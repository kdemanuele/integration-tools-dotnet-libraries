using FluentAssertions;
using NUnit.Framework;
using IntegrationTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Transform.UnitTest
{
    public class TransformationRuileLogicTests
    {
        [Test]
        public void ConversionTypeRuleOperationCheck()
        {
            RuleSetCollection<TransformationRule> ruleSetCollection = new RuleSetCollection<TransformationRule>().InitialiseRuleSet("test");

            ruleSetCollection["test"].Add("unittest", new()
            {
                new() { SourceField = "sourceField1", TargetField = "targetField1" },
                new() { SourceField = "sourceField2", TargetField = "targetField2", AsType = ITransformationRule.As.IsoDate },
                new() { SourceField = "sourceField2", TargetField = "targetField3", AsType = ITransformationRule.As.IsoUtcDate },
                new() { SourceField = "sourceField3", TargetField = "targetField4", AsType = ITransformationRule.As.Double },
                new() { SourceField = "sourceField4", TargetField = "targetField5", AsType = ITransformationRule.As.WholeNumber },
                new() { SourceField = "sourceField5", TargetField = "targetField6", AsType = ITransformationRule.As.SerializeJson },
                new() { SourceField = "sourceField6", TargetField = "targetField7", AsType = ITransformationRule.As.String },
            });

            ruleSetCollection["test"]["unittest"].Should().ContainSingle(rule => rule.TargetField == "targetField1").Which.Operation.Should().Be(ITransformationRule.TransformType.AS_IS);
            ruleSetCollection["test"]["unittest"].Should().ContainSingle(rule => rule.TargetField == "targetField2").Which.Operation.Should().Be(ITransformationRule.TransformType.TYPE_CONVERSION);
            ruleSetCollection["test"]["unittest"].Should().ContainSingle(rule => rule.TargetField == "targetField3").Which.Operation.Should().Be(ITransformationRule.TransformType.TYPE_CONVERSION);
            ruleSetCollection["test"]["unittest"].Should().ContainSingle(rule => rule.TargetField == "targetField4").Which.Operation.Should().Be(ITransformationRule.TransformType.TYPE_CONVERSION);
            ruleSetCollection["test"]["unittest"].Should().ContainSingle(rule => rule.TargetField == "targetField5").Which.Operation.Should().Be(ITransformationRule.TransformType.TYPE_CONVERSION);
            ruleSetCollection["test"]["unittest"].Should().ContainSingle(rule => rule.TargetField == "targetField6").Which.Operation.Should().Be(ITransformationRule.TransformType.TYPE_CONVERSION);
            ruleSetCollection["test"]["unittest"].Should().ContainSingle(rule => rule.TargetField == "targetField7").Which.Operation.Should().Be(ITransformationRule.TransformType.TYPE_CONVERSION);
        }

        [Test]
        public void ExpressionRuleOperationCheck()
        {
            RuleSetCollection<TransformationRule> ruleSetCollection = new RuleSetCollection<TransformationRule>().InitialiseRuleSet("test");

            ruleSetCollection["test"].Add("unittest", new()
            {
                new() { SourceField = "sourceField1", TargetField = "fixedString", Expression = (data, log) => "Got A Value" },
                new() { SourceField = "sourceField2", TargetField = "tomorrow", AsType = ITransformationRule.As.IsoDate, Expression = (data, log) => ((DateTime)data).AddDays(1) },
                new() { TargetField = "addingNumbers", Expression = (data, log) => data["value1"] + data["value2"] },
            });

            ruleSetCollection["test"]["unittest"].Should().ContainSingle(rule => rule.TargetField == "fixedString").Which.Operation.Should().Be(ITransformationRule.TransformType.EXPRESSION);
            ruleSetCollection["test"]["unittest"].Should().ContainSingle(rule => rule.TargetField == "tomorrow").Which.Operation.Should().Be(ITransformationRule.TransformType.EXPRESSION | ITransformationRule.TransformType.TYPE_CONVERSION);
            ruleSetCollection["test"]["unittest"].Should().ContainSingle(rule => rule.TargetField == "addingNumbers").Which.Operation.Should().Be(ITransformationRule.TransformType.EXPRESSION);
        }

        [Test]
        public void ValueSetOperationCheck()
        {
            RuleSetCollection<TransformationRule> ruleSetCollection = new RuleSetCollection<TransformationRule>().InitialiseRuleSet("test");

            ruleSetCollection["test"].Add("unittest", new()
            {
                new()
                {
                    SourceField = "sourceField1",
                    TargetField = "targetField1"
                },
                new()
                {
                    SourceField = "sourceField2",
                    TargetField = "targetField2",
                    ValueSetMatch = new ValueSetCompareRule("unittest", ValueSetCompareRule.MatchOn.Label, false)
                },
                new()
                {
                    SourceField = "sourceField3",
                    TargetField = "targetField3",
                    ValueSetMatch = new ValueSetCompareRule("unittest", ValueSetCompareRule.MatchOn.Label, false),
                    AsType = ITransformationRule.As.WholeNumber
                },
                new()
                {
                    SourceField = "sourceField4",
                    TargetField = "targetField4",
                    ValueSetMatch = new ValueSetCompareRule("unittest", ValueSetCompareRule.MatchOn.Label, false),
                    Expression = (data, log) => "Something"
                },
                new()
                {
                    SourceField = "sourceField5",
                    TargetField = "targetField5",
                    ValueSetMatch = new ValueSetCompareRule("unittest", ValueSetCompareRule.MatchOn.Label, false),
                    Expression = (data, log) => "Something",
                    AsType = ITransformationRule.As.WholeNumber
                },
            });

            ruleSetCollection["test"]["unittest"].Should()
                .ContainSingle(rule => rule.TargetField == "targetField1")
                .Which.Operation.Should()
                .Be(ITransformationRule.TransformType.AS_IS);

            ruleSetCollection["test"]["unittest"].Should()
                .ContainSingle(rule => rule.TargetField == "targetField2")
                .Which.Operation.Should()
                .Be(ITransformationRule.TransformType.VALUE_CONVERSION);

            ruleSetCollection["test"]["unittest"].Should()
                .ContainSingle(rule => rule.TargetField == "targetField3")
                .Which.Operation.Should()
                .Be(ITransformationRule.TransformType.VALUE_CONVERSION | ITransformationRule.TransformType.TYPE_CONVERSION);

            ruleSetCollection["test"]["unittest"].Should()
                .ContainSingle(rule => rule.TargetField == "targetField4")
                .Which.Operation.Should()
                .Be(ITransformationRule.TransformType.VALUE_CONVERSION | ITransformationRule.TransformType.EXPRESSION);

            ruleSetCollection["test"]["unittest"].Should()
                .ContainSingle(rule => rule.TargetField == "targetField5")
                .Which.Operation.Should()
                .Be(ITransformationRule.TransformType.VALUE_CONVERSION | ITransformationRule.TransformType.TYPE_CONVERSION | ITransformationRule.TransformType.EXPRESSION);
        }
    }
}
