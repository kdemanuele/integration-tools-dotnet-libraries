using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Config;
using NUnit.Framework;
using IntegrationTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Transform.UnitTest
{
    public class TransformationValueSetTests
    {
        private IntegrationLogger integrationLogger;
        private MemoryAppender log4netMemory;

        [SetUp]
        public void SetLog4Net()
        {
            log4netMemory = new MemoryAppender();
            BasicConfigurator.Configure(log4netMemory);
            ILog log = LogManager.GetLogger(typeof(TransformTests));
            integrationLogger = new(log);
        }

        [Test]
        public void StringValueTransformation()
        {
            ValueSet valueSet = new();
            valueSet.AddValueToSet("test", "label1", "value1");
            valueSet.AddValueToSet("test", "label2", "value2");
            valueSet.AddValueToSet("test", "label3", "value3");

            RuleSetCollection<TransformationRule> ruleSetCollection = new();
            ruleSetCollection.InitialiseRuleSet("UnitTest");
            ruleSetCollection["UnitTest"].TryAdd("Testing", new()
            {
                new() { SourceField = "data1", TargetField = "valueMatchedField1", ValueSetMatch = new ValueSetCompareRule("test", ValueSetCompareRule.MatchOn.Label, true) },
                new() { SourceField = "data2", TargetField = "valueMatchedField2", ValueSetMatch = new ValueSetCompareRule("test", ValueSetCompareRule.MatchOn.Value, true) },
                new() { SourceField = "data3", TargetField = "valueMatchedField3", ValueSetMatch = new ValueSetCompareRule("test", ValueSetCompareRule.MatchOn.Label, false) },
                new() { SourceField = "data3", TargetField = "valueMatchedField4", ValueSetMatch = new ValueSetCompareRule("test", ValueSetCompareRule.MatchOn.Label, true), Expression = (data, log) => data.ToLower() },
                new() { TargetField = "valueMatchedField5", ValueSetMatch = new ValueSetCompareRule("test", ValueSetCompareRule.MatchOn.Label, false), Expression = (data, log) => data["field1"].Value.ToUpper() + data["field2"] }
            });

            var source = new
            {
                data1 = "label2",
                data2 = "value3",
                data3 = "LABEL1",
                field1 = "label",
                field2 = 3
            };

            Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection, ValueSet = valueSet };

            dynamic targetData = null;

            transform.Invoking(instance => targetData = transform.PerformTransformation(source, "UnitTest"))
                .Should().NotThrow();

            var targetDictionary = (IDictionary<string, object>)targetData["Testing"][0];

            targetDictionary.Should().ContainKeys("valueMatchedField1", "valueMatchedField2", "valueMatchedField3", "valueMatchedField4", "valueMatchedField5");
            targetDictionary["valueMatchedField1"].Should().BeEquivalentTo("value2");
            targetDictionary["valueMatchedField2"].Should().BeEquivalentTo("label3");
            targetDictionary["valueMatchedField3"].Should().BeEquivalentTo("value1");
            targetDictionary["valueMatchedField4"].Should().BeEquivalentTo("value1");
            targetDictionary["valueMatchedField5"].Should().BeEquivalentTo("value3");
        }

        [Test]
        public void ConvertionValueTransformation()
        {
            ValueSet valueSet = new();
            valueSet.AddValueToSet("test", "label1", "3333-3333-3333-3333");
            valueSet.AddValueToSet("test", "label2", "1234");
            valueSet.AddValueToSet("test", "label3", "1234.1234");

            RuleSetCollection<TransformationRule> ruleSetCollection = new();
            ruleSetCollection.InitialiseRuleSet("UnitTest");
            ruleSetCollection["UnitTest"].TryAdd("Testing", new()
            {
                new() { SourceField = "data1", TargetField = "valueMatchedField1", ValueSetMatch = new ValueSetCompareRule("test", ValueSetCompareRule.MatchOn.Label, true), AsType = ITransformationRule.As.WholeNumber },
                new() { SourceField = "data1", TargetField = "valueMatchedField2", ValueSetMatch = new ValueSetCompareRule("test", ValueSetCompareRule.MatchOn.Label, true), AsType = ITransformationRule.As.String },
                new() { SourceField = "data3", TargetField = "valueMatchedField3", ValueSetMatch = new ValueSetCompareRule("test", ValueSetCompareRule.MatchOn.Label, false), AsType = ITransformationRule.As.Double },
                new() { SourceField = "data3", TargetField = "valueMatchedField4", ValueSetMatch = new ValueSetCompareRule("test", ValueSetCompareRule.MatchOn.Label, true), Expression = (data, log) => data.ToLower(), AsType = ITransformationRule.As.Double },
                new() { SourceField = "data2", TargetField = "valueMatchedField5", ValueSetMatch = new ValueSetCompareRule("test", ValueSetCompareRule.MatchOn.Label, true), Expression = (data, log) => data.ToLower(), AsType = ITransformationRule.As.WholeNumber }
            });

            var source = new
            {
                data1 = "label2",
                data2 = "label1",
                data3 = "LABEL3"
            };

            Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection, ValueSet = valueSet };

            dynamic targetData = null;

            transform.Invoking(instance => targetData = transform.PerformTransformation(source, "UnitTest"))
                .Should().NotThrow();

            var targetDictionary = (IDictionary<string, object>)targetData["Testing"][0];

            targetDictionary.Should().ContainKeys("valueMatchedField1", "valueMatchedField2", "valueMatchedField3", "valueMatchedField4");
            targetDictionary.Should().NotContainKey("valueMatchedField5");
            targetDictionary["valueMatchedField1"].Should().BeOfType<long>().And.Subject.As<long>().Should().Be(1234);
            targetDictionary["valueMatchedField2"].Should().BeOfType<string>().And.Subject.Should().BeEquivalentTo("1234");
            targetDictionary["valueMatchedField3"].Should().BeOfType<double>().And.Subject.Should().Be(1234.1234);
            targetDictionary["valueMatchedField4"].Should().BeOfType<double>().And.Subject.Should().Be(1234.1234);
        }


    }
}
