using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IntegrationTool.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntegrationTool.Validation;

public class Validate : IValidate<ValidationRule>
{
    public IIntegrationLogger Logger { get; init; }
    public RuleSetCollection<ValidationRule> RuleSets { get; init; }

    public bool PerformValidation(dynamic source, string rulesetName)
    {
        if (RuleSets is null)
        {
            throw new ValidationRuleException("No Rule Sets have been configured against which to perform validation");
        }

        if (source is null)
        {
            throw new ValidationRuleException("The validation cannot be performed on null objects");
        }

        if (string.IsNullOrEmpty(rulesetName))
        {
            throw new ValidationRuleException("A Ruleset needs to be defined against which validation rules are to be checked");
        }

        bool isValid = true;
        Stopwatch stopwatch = new();
        Logger?.LogDebug("Initialising Validation Process");
#if PERFTEST
            stopwatch.Start();
#endif

        var sourceJson = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(source));

        Parallel.ForEach(RuleSets[rulesetName], (validation) =>
        {
            isValid &= IsValid(sourceJson, validation.Value);
        });

#if PERFTEST
            Logger?.LogInfo($"Validation Process Execution Time: {stopwatch.ElapsedMilliseconds}ms");
#endif
        Logger?.LogDebug("Validation Process Ended");
#if PERFTEST
            stopwatch.Stop();
#endif

        return isValid;
    }

    private bool IsValid(JToken source, List<ValidationRule> validation)
    {
        bool isValid = true;

        if (source is JArray)
        {
            Parallel.ForEach(source, (token) =>
            {
                isValid &= IsValid(token, validation);
            });
        }

        if (source is JObject sourceObject)
        {
            isValid = ValidateObject(sourceObject, validation);
        }

        return isValid;
    }

    private bool ValidateObject(JObject source, List<ValidationRule> validation)
    {
        bool isValid = true;

        foreach (var rule in validation)
        {
            if (rule.Field is not null && rule.IsRequired)
            {
                if (!(isValid &= (source[rule.Field] is not null)))
                {
                    Logger.LogError($"The {rule.Field} field is required but an object was passed with the field missing");
                }
            }

            if (isValid && rule.Field is not null && !string.IsNullOrWhiteSpace(rule.Pattern) && source[rule.Field] is not null)
            {
                if (!(isValid &= Regex.IsMatch(source[rule.Field].ToString(), $"\\b{rule.Pattern}\\b")))
                {
                    Logger.LogError($"The {rule.Field} field is expected to match the pattern {rule.Pattern} but the field value '{source[rule.Field]}' fails the test");
                }
            }

            if (rule.Expression is not null)
            {
                if (rule.Field is not null && source[rule.Field] is not null && !(isValid &= rule.Expression.Invoke(source.Value<JValue>(rule.Field).Value, Logger)))
                {
                    Logger.LogError($"The {rule.Field} field has failed the evaluation of the validation rule.");
                }

                if (rule.Field is null && !(isValid &= rule.Expression.Invoke(source, Logger)))
                {
                    Logger.LogError($"An evaluation expression has failed to validate the input.");
                }
            }
        }

        return isValid;
    }
}
