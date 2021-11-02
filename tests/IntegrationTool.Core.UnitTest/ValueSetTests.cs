using NUnit.Framework;
using FluentAssertions;
using System.Data;
using System;

namespace IntegrationTool.Core.UnitTest
{
    public class ValueSetTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void AddingOfValueToSet()
        {
            ValueSet valueSet = new();
            valueSet
                .Invoking(instance => instance.AddValueToSet("unit_test", "label", "1234-1234"))
                .Should().NotThrow("the set should be automatically created and assigned the new value");

            valueSet
                .Invoking(instance => instance.AddValueToSet("unit_test", "label2", "1236-1236"))
                .Should().NotThrow("the set should add the new value");

            valueSet
                .Invoking(instance => instance.AddValueToSet("unit_test", "label2", "1234-1234"))
                .Should().NotThrow("the set should accept the same label or value");
        }

        [Test]
        public void GetValueSet()
        {
            ValueSet valueSet = new();
            valueSet.AddValueToSet("unit_test", "label", "1234-1234");
            valueSet.AddValueToSet("unit_test", "label2", "1236-1236");
            valueSet.AddValueToSet("unit_test", "label2", "1234-1234");

            valueSet
                .Invoking(instance => instance.GetValueSet("non_existing"))
                .Should().Throw<TransformationRuleException>(because: "the set doesn't exist");

            valueSet
                .Invoking(instance => instance.GetValueSet("unit_test"))
                .Should().NotThrow("the set should have been initialised");

            valueSet.GetValueSet("unit_test").Rows.Count.Should().Be(3);
        }

        [Test]
        public void CheckReturnOfMatchedValues()
        {
            ValueSet valueSet = new();
            valueSet.AddValueToSet("unit_test", "label", "1234-1234");
            valueSet.AddValueToSet("unit_test", "label2", "1236-1236");
            valueSet.AddValueToSet("unit_test", "label2", "1234-1234");

            valueSet
                .Invoking(instance => instance.GetLabelAndValue("non_existing", "label", ValueSetCompareRule.MatchOn.Label, true))
                .Should().Throw<TransformationRuleException>(because: "the set doesn't exist");

            valueSet.GetLabelAndValue("unit_test", "label", ValueSetCompareRule.MatchOn.Label, true)
                .Should().BeEquivalentTo(Tuple.Create("label", "1234-1234"));

            valueSet.GetLabelAndValue("unit_test", "1234-1234", ValueSetCompareRule.MatchOn.Value, true)
                .Should().BeEquivalentTo(Tuple.Create("label", "1234-1234"));

            valueSet.GetLabelAndValue("unit_test", "1236-1236", ValueSetCompareRule.MatchOn.Value, true)
                .Should().BeEquivalentTo(Tuple.Create("label2", "1236-1236"));

            valueSet.GetLabelAndValue("unit_test", "label2", ValueSetCompareRule.MatchOn.Label, true)
                .Should().BeEquivalentTo(Tuple.Create("label2", "1236-1236"));

            valueSet.GetLabelAndValue("unit_test", "label15", ValueSetCompareRule.MatchOn.Label, true)
                .Should().BeEquivalentTo(Tuple.Create<string, string>(null, null));

            valueSet.GetLabelAndValue("unit_test", "Label2", ValueSetCompareRule.MatchOn.Label, true)
                .Should().BeEquivalentTo(Tuple.Create<string, string>(null, null));

            valueSet.GetLabelAndValue("unit_test", "Label2", ValueSetCompareRule.MatchOn.Label, false)
                .Should().BeEquivalentTo(Tuple.Create("label2", "1236-1236"));
        }
    }
}