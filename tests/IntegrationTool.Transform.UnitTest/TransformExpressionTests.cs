using IntegrationTool.Core;
using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Transform.UnitTest
{
    public class TransformExpressionTests
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
        public void ExpressionEvaluation()
        {
            RuleSetCollection<TransformationRule> ruleSetCollection = new RuleSetCollection<TransformationRule>().InitialiseRuleSet("test");

            ruleSetCollection["test"].Add("unittest", new()
            {
                new TransformationRule { SourceField = "sourceField1", TargetField = "fixedString", Expression = (data, log) => "Got A Value" },
                new TransformationRule { SourceField = "sourceField2", TargetField = "sameString", Expression = (data, log) => data.ToLower() },
                new TransformationRule { SourceField = "sourceField3", TargetField = "tomorrow", AsType = ITransformationRule.As.IsoUtcDate, Expression = (data, log) => data.AddDays(1) },
                new TransformationRule { TargetField = "addingNumbers", Expression = (data, log) => data["value1"] + data["value2"] },
            });

            Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };

            var source = new
            {
                sourceField1 = "Ignored Value",
                sourceField2 = "Lowered STRING",
                sourceField3 = DateTime.Today,
                value1 = 231,
                value2 = 123
            };

            dynamic targetData = null;
            Action action = () => targetData = transform.PerformTransformation(source, "test");

            action.Should().NotThrow();

            var targetObject = targetData["unittest"][0];
            Assert.IsInstanceOf<string>(targetObject.fixedString);
            Assert.IsInstanceOf<string>(targetObject.sameString);
            Assert.IsInstanceOf<string>(targetObject.tomorrow);
            Assert.IsInstanceOf<long>(targetObject.addingNumbers);
            ((string)targetObject.fixedString).Should().Be("Got A Value");
            ((string)targetObject.sameString).Should().Be("lowered string");
            ((string)targetObject.tomorrow).Should().Be(DateTime.Today.AddDays(1).ToUniversalTime().ToString("O"));
            ((int)targetObject.addingNumbers).Should().Be(354);
        }
    }
}
