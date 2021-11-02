using IntegrationTool.Core;
using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Config;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace IntegrationTool.Transform.UnitTest
{
    public class TransformTests
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
        public void CanRunTransformationForCompleteRuleSet()
        {
            RuleSetCollection<TransformationRule> ruleSetCollection = new();
            ruleSetCollection.InitialiseRuleSet("Fake");
            ruleSetCollection["Fake"].Add("FakeRule", new() { new() { SourceField = "Input", TargetField = "Output" } });
            Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };

            transform.CanRunTransformationForRuleSet("Fake").Should().BeTrue();
        }

        [Test]
        public void CanRunTransformationForMissingRuleSet()
        {
            RuleSetCollection<TransformationRule> ruleSetCollection = new();
            ruleSetCollection.InitialiseRuleSet("Fake");
            ruleSetCollection["Fake"].Add("FakeRule", null);
            Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };

            transform.CanRunTransformationForRuleSet("InvalidFake").Should().BeFalse();
        }

        [Test]
        public void CanRunTransformationForIncompleteRuleSet()
        {
            RuleSetCollection<TransformationRule> ruleSetCollection = new();
            ruleSetCollection.InitialiseRuleSet("Fake");
            Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };

            transform.CanRunTransformationForRuleSet("Fake").Should().BeFalse();
        }
    }
}