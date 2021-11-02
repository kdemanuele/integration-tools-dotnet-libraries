using IntegrationTool.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;

namespace IntegrationTool.Transform
{
    public class Transform : ITransform<TransformationRule>
    {
        public ValueSet ValueSet { get; init; }
        public IIntegrationLogger Logger { get; init; }
        public RuleSetCollection<TransformationRule> RuleSets { get; init; }

        public dynamic PerformTransformation(dynamic source, string rulesetName)
        {
            Stopwatch stopwatch = new();
            Logger?.LogDebug("Initialising Transformation Process");
#if PERFTEST
            stopwatch.Start();
#endif
            if (CanRunTransformationForRuleSet(rulesetName))
            {
                Logger?.LogDebug("Rule Set is valid for processing");
            }
            
            Dictionary<string, List<ExpandoObject>> transformedObjects = new();
            var sourceJson = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(source));

            Parallel.ForEach(RuleSets[rulesetName], (transformation) =>
            {
                transformedObjects.Add(transformation.Key, new());

                Transformation(sourceJson, transformation.Value, transformedObjects[transformation.Key]);
            });

#if PERFTEST
            Logger?.LogInfo($"Transformation Process Execution Time: {stopwatch.ElapsedMilliseconds}ms");
#endif
            Logger?.LogDebug("Transformation Process Ended");
#if PERFTEST
            stopwatch.Stop();
#endif
            return transformedObjects;
        }

        private void Transformation(JToken source, List<TransformationRule> transformation, List<ExpandoObject> objects)
        {
            if (source is JArray)
            {
                foreach (var sourceObject in source)
                {
                    Transformation(sourceObject, transformation, objects);
                }
            }

            if (source is JObject srcObject)
            {
                ExpandoObject transformedObject = new();
                IDictionary<string, object> transformedObjectProperties = transformedObject;
                TransformObject(srcObject, transformation, transformedObjectProperties);

                if (transformedObjectProperties.Count > 0)
                {
                    objects.Add(transformedObject);
                }
            }
        }

        private void TransformObject(JObject source, List<TransformationRule> transformation, IDictionary<string, object> transformedObjectProperties)
        {
            foreach (var rule in transformation)
            {
                if (string.IsNullOrWhiteSpace(rule.TargetField))
                {
                    throw new TransformationRuleException("The Target Field for one of the rules is missing.");
                }

                object newProperty = rule.Operation switch
                {
                    ITransformationRule.TransformType.TYPE_CONVERSION => ConvertObjectFieldType(source, rule),
                    ITransformationRule.TransformType.EXPRESSION => EvaluateExpression(source, rule),
                    ITransformationRule.TransformType.TYPE_CONVERSION |
                        ITransformationRule.TransformType.EXPRESSION => ConvertFieldType(EvaluateExpression(source, rule), rule),
                    ITransformationRule.TransformType.VALUE_CONVERSION => ConvertObjectValue(source, rule),
                    ITransformationRule.TransformType.TYPE_CONVERSION |
                        ITransformationRule.TransformType.VALUE_CONVERSION => ConvertFieldType(ConvertObjectValue(source, rule), rule),
                    ITransformationRule.TransformType.VALUE_CONVERSION |
                        ITransformationRule.TransformType.EXPRESSION => ConvertValue(EvaluateExpression(source, rule), rule),
                    ITransformationRule.TransformType.TYPE_CONVERSION |
                        ITransformationRule.TransformType.EXPRESSION |
                        ITransformationRule.TransformType.VALUE_CONVERSION => ConvertFieldType(ConvertValue(EvaluateExpression(source, rule), rule), rule),
                    _ => LeaveFieldDataAsIs(source, rule)
                };

                if (newProperty is not null)
                {
                    transformedObjectProperties[rule.TargetField] = newProperty;
                }
            }
        }

        private object ConvertObjectFieldType(dynamic source, TransformationRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.SourceField))
            {
                throw new TransformationRuleException($"The Source Field for the target value {rule.TargetField} is missing.");
            }

            return ConvertFieldType(source[rule.SourceField], rule);
        }

        private object ConvertFieldType(dynamic sourceField, TransformationRule rule)
        {
            return rule.AsType switch
            {
                ITransformationRule.As.Double => TypeConverter.ConvertToDouble(sourceField, rule, Logger),
                ITransformationRule.As.IsoDate => TypeConverter.ConvertToIsoDateTime(sourceField, rule, Logger),
                ITransformationRule.As.IsoUtcDate => TypeConverter.ConvertToIsoUtcDateTime(sourceField, rule, Logger),
                ITransformationRule.As.SerializeJson => TypeConverter.SerializeJson(sourceField, rule, Logger),
                ITransformationRule.As.String => sourceField?.ToString(),
                ITransformationRule.As.WholeNumber => TypeConverter.ConvertToLong(sourceField, rule, Logger),
                _ => throw new TransformationRuleException($"The conversion type, {rule.AsType}, specified is not implemented for {rule.TargetField}")
            };
        }

        private dynamic EvaluateExpression(dynamic source, TransformationRule rule)
        {
            dynamic value;

            if (string.IsNullOrEmpty(rule.SourceField))
            {
                value = rule.Expression.Invoke(source, Logger);
            }
            else
            {
                value = rule.Expression.Invoke(source[rule.SourceField].Value, Logger);
            }

            if (value is JValue && value is not null)
            {
                return value.Value;
            }

            return value;
        }

        private string ConvertObjectValue(dynamic source, TransformationRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.SourceField))
            {
                throw new TransformationRuleException($"The Source Field for the target value {rule.TargetField} is missing.");
            }

            return ConvertValue(source[rule.SourceField], rule);
        }

        private string ConvertValue(dynamic source, TransformationRule rule)
        {
            var valueSetMatch = rule.ValueSetMatch.Value;
            var sourceString = source.ToString();

            var valueSetData = ValueSet.GetLabelAndValue(
                valueSetMatch.SetName, 
                sourceString, 
                valueSetMatch.MatchRule, 
                valueSetMatch.IsCaseSensitiveMatch);

            if (valueSetData.Item1 is null && valueSetData.Item2 is null)
            {
                return null;
            }
                
            if (valueSetMatch.MatchRule == ValueSetCompareRule.MatchOn.Label)
            {
                return valueSetData.Item2;
            }

            return valueSetData.Item1;
        }

        private static object LeaveFieldDataAsIs(dynamic source, TransformationRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.SourceField))
            {
                throw new MissingFieldException($"The Source Field for Target Field, {rule.TargetField}, is missing.");
            }

            object newProperty = null;

            if (source[rule.SourceField] is not null)
            {
                newProperty = (source[rule.SourceField] as JValue)?.Value;
            }

            return newProperty;
        }

        /// <summary>
        /// Validate the Rule Set has been properly initialised for transformation
        /// </summary>
        /// <param name="rulesetName">Name of the Rule Set to use for transformation</param>
        /// <returns>True if the initialisation is complete</returns>
        public bool CanRunTransformationForRuleSet(string rulesetName)
        {
            bool canRun = true;
            if (RuleSets.GetRuleSetState(rulesetName) is RuleSetState.Undeclared)
            {
                Logger?.LogError("The Transformation Process was called with an unknown Rule Set");
                canRun &= false;
            }

            if (RuleSets.GetRuleSetState(rulesetName) is RuleSetState.No_Targets_Defined)
            {
                Logger?.LogError("The Transformation Process was called with a Rule Set that has no targets for transformation rules");
                canRun &= false;
            }

            if (RuleSets.GetRuleSetState(rulesetName) is RuleSetState.Target_Without_Rules_Defined)
            {
                Logger?.LogError("The Transformation Process was called with a Rule Set that has a target without transformation rules");
                canRun &= false;
            }

            return canRun;
        }
    }
}
