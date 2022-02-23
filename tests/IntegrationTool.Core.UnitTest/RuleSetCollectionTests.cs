using FluentAssertions;
using NUnit.Framework;
using IntegrationTool.Core.UnitTest.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTool.Core.UnitTest;

class RuleSetCollectionTests
{
    [Test]
    public void InitialisingWithoutRuleSets()
    {
        Action action = () => new RuleSetCollection<ITransformationRule>().InitialiseRuleSet(null);

        action.Should().Throw<ArgumentNullException>("Null should not be accepted as a rule set name").WithMessage("Rule Set name cannot be null (Parameter 'rulesetName')");
    }

    [Test]
    public void InitialisingWithRuleSetOnly()
    {
        RuleSetCollection<ITransformationRule> ruleSetCollection = new();

        Action action = () => ruleSetCollection.InitialiseRuleSet("test1");

        action.Should().NotThrow();
        action.Should().NotBeNull();

        ruleSetCollection.InitialiseRuleSet("test2");
        Action action1 = () => ruleSetCollection["test1"].Should().NotBeNull();
        action1.Should().NotThrow<KeyNotFoundException>("Previous rule sets should not be removed");
    }

    [Test]
    public void InitialisingWithRuleSetTargets()
    {
        RuleSetCollection<ITransformationRule> ruleSetCollection = null;

        Action action = () => ruleSetCollection = new RuleSetCollection<ITransformationRule>().InitialiseRuleSet("test1", "target1", "target2", "target3");

        action.Should().NotThrow();
        action.Should().NotBeNull();
        ruleSetCollection["test1"].Count.Should().Be(3);
        ruleSetCollection["test1"].ContainsKey("target1").Should().BeTrue();
        ruleSetCollection["test1"].ContainsKey("target2").Should().BeTrue();
        ruleSetCollection["test1"].ContainsKey("target3").Should().BeTrue();
        ruleSetCollection["test1"].ContainsKey("target4").Should().BeFalse();
    }

    [Test]
    public void CheckForStateOfUndefinedRuleset()
    {
        RuleSetCollection<ITransformationRule> ruleSetCollection = new RuleSetCollection<ITransformationRule>().InitialiseRuleSet("test1", "target1", "target2", "target3");

        ruleSetCollection.GetRuleSetState("Fake").Should().Be(RuleSetState.Undeclared);
    }

    [Test]
    public void CheckForStateOfNoTargetsInRuleset()
    {
        RuleSetCollection<ITransformationRule> ruleSetCollection = new RuleSetCollection<ITransformationRule>().InitialiseRuleSet("test1");

        ruleSetCollection.GetRuleSetState("test1").Should().Be(RuleSetState.No_Targets_Defined);
    }

    [Test]
    public void CheckForStateOfMissingRulesInRuleset()
    {
        RuleSetCollection<MockTransformationRule> ruleSetCollection = new RuleSetCollection<MockTransformationRule>().InitialiseRuleSet("test1", "target1");

        ruleSetCollection.GetRuleSetState("test1").Should().Be(RuleSetState.Target_Without_Rules_Defined);

        ruleSetCollection.InitialiseRuleSet("test1", "InvalidRule");

        ruleSetCollection["test1"]["target1"] = new()
        {
            new() { SourceField = "sourceField1", TargetField = "targetField1" },
            new() { SourceField = "sourceField2", TargetField = "targetField2" },
            new() { SourceField = "sourceField3", TargetField = "targetField3" },
        };
        ruleSetCollection["test1"].Add("target2", new() { new() { SourceField = "source", TargetField = "target" } });

        ruleSetCollection["test1"]["target1"].Count.Should().Be(3);
        ruleSetCollection["test1"]["target2"].Count.Should().Be(1);
        ruleSetCollection.GetRuleSetState("test1").Should().Be(RuleSetState.Target_Without_Rules_Defined);
    }
}
