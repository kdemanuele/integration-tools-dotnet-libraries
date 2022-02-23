using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Config;
using NUnit.Framework;
using IntegrationTool.Core;
using IntegrationTool.Validation;
using System;
using System.Linq;

namespace IntegrationTool.Validation.UnitTest;

public class ValidateTests
{
    private IntegrationLogger integrationLogger;
    private MemoryAppender log4netMemory;

    [SetUp]
    public void SetLog4Net()
    {
        log4netMemory = new MemoryAppender();
        BasicConfigurator.Configure(log4netMemory);
        ILog log = LogManager.GetLogger(typeof(ValidateTests));
        integrationLogger = new(log);
    }

    [Test]
    public void PerformValidationNoErrors()
    {
        Validate validater = new() { Logger = integrationLogger };

        validater.Invoking(instance => instance.PerformValidation(new { }, string.Empty))
            .Should().Throw<ValidationRuleException>().WithMessage("No Rule Sets have been configured against which to perform validation");

        validater = new() { Logger = integrationLogger, RuleSets = new() { } };
        validater.Invoking(instance => instance.PerformValidation(null, string.Empty))
            .Should().Throw<ValidationRuleException>().WithMessage("The validation cannot be performed on null objects");

        validater.Invoking(instance => instance.PerformValidation(new { }, string.Empty))
            .Should().Throw<ValidationRuleException>().WithMessage("A Ruleset needs to be defined against which validation rules are to be checked");

        validater.Invoking(instance => instance.PerformValidation(new { }, "fake"))
            .Should().NotThrow<ValidationRuleException>();
    }

    [Test]
    public void PerformValidationOnArraysOrObject()
    {
        RuleSetCollection<ValidationRule> validationRuleSets = new();
        validationRuleSets.InitialiseRuleSet("unit_test");
        validationRuleSets["unit_test"].Add("test", new()
        {
            new() { Field = "InnerObject" }
        });

        var source = new object[] {
                new { InnerObject = 1 },
                new { InnerObject = 2 },
                new { InnerObject = 3 },
                new { InnerObject = 4 }
            };

        bool validInput = false;

        Validate validater = new() { Logger = integrationLogger, RuleSets = validationRuleSets };
        validater.Invoking(instance => validInput = instance.PerformValidation(source, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        validInput.Should().BeTrue();

        var sourceObject = new
        {
            InnerObject = 1
        };

        validInput = false;
        validater.Invoking(instance => validInput = instance.PerformValidation(sourceObject, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        validInput.Should().BeTrue();
    }

    [Test]
    public void IsRequiredValidation()
    {
        RuleSetCollection<ValidationRule> validationRuleSets = new();
        validationRuleSets.InitialiseRuleSet("unit_test");
        validationRuleSets["unit_test"].Add("test", new()
        {
            new() { Field = "InnerObject", IsRequired = true }
        });

        var source = new object[] {
                new { InnerObject = 1 },
                new { InnerObject = 2 },
                new { InnerObject = 3 },
                new { InnerObject = 4 }
            };

        bool isValidInput = false;

        Validate validater = new() { Logger = integrationLogger, RuleSets = validationRuleSets };
        validater.Invoking(instance => isValidInput = instance.PerformValidation(source, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        isValidInput.Should().BeTrue();
        var events = log4netMemory.GetEvents();
        events.Any(ev => ev.Level == log4net.Core.Level.Error).Should().BeFalse();

        var sourceObject = new object[]
        {
                new { InnerObject = 1 },
                new { InnerObject2 = "sometext"}
        };

        validater.Invoking(instance => isValidInput = instance.PerformValidation(sourceObject, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        isValidInput.Should().BeFalse();
        events = log4netMemory.GetEvents().Where(ev => ev.Level == log4net.Core.Level.Error).ToArray();
        events.Length.Should().Be(1);
        events.FirstOrDefault().RenderedMessage.Should().StartWith("The InnerObject field is required but an object was passed with the field missing");
    }

    [Test]
    public void PatternValidation()
    {
        RuleSetCollection<ValidationRule> validationRuleSets = new();
        validationRuleSets.InitialiseRuleSet("unit_test");
        validationRuleSets["unit_test"].Add("test", new()
        {
            new() { Field = "InnerObject", Pattern = "\\d{6,8}" }
        });

        var source = new object[] {
                new { InnerObject = 123456 },
                new { InnerObject = 2345678 },
                new { InnerObject = 34567891 }
            };

        bool isValidInput = false;
        Validate validater = new() { Logger = integrationLogger, RuleSets = validationRuleSets };
        validater.Invoking(instance => isValidInput = instance.PerformValidation(source, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        isValidInput.Should().BeTrue();
        var events = log4netMemory.GetEvents();
        events.Any(ev => ev.Level == log4net.Core.Level.Error).Should().BeFalse();

        var sourceObject = new object[]
        {
                new { InnerObject = 12345 },
                new { InnerObject = 123456789 }
        };

        validater.Invoking(instance => isValidInput = instance.PerformValidation(sourceObject, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        isValidInput.Should().BeFalse();
        events = log4netMemory.GetEvents().Where(ev => ev.Level == log4net.Core.Level.Error).ToArray();
        events.Length.Should().Be(2);
        events.Any(ev => ev.RenderedMessage.StartsWith("The InnerObject field is expected to match the pattern \\d{6,8} but the field value '12345' fails the test")).Should().BeTrue();
        events.Any(ev => ev.RenderedMessage.StartsWith("The InnerObject field is expected to match the pattern \\d{6,8} but the field value '123456789' fails the test")).Should().BeTrue();
    }

    [Test]
    public void RequiredWithPatternValidation()
    {
        RuleSetCollection<ValidationRule> validationRuleSets = new();
        validationRuleSets.InitialiseRuleSet("unit_test");
        validationRuleSets["unit_test"].Add("test", new()
        {
            new() { Field = "InnerObject", Pattern = "\\d{6,8}", IsRequired = true }
        });

        var source = new object[] {
                new { InnerObject = 123456 },
                new { InnerObject = 2345678 },
                new { InnerObject = 34567891 }
            };

        bool isValidInput = false;
        Validate validater = new() { Logger = integrationLogger, RuleSets = validationRuleSets };
        validater.Invoking(instance => isValidInput = instance.PerformValidation(source, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        isValidInput.Should().BeTrue();
        var events = log4netMemory.GetEvents();
        events.Any(ev => ev.Level == log4net.Core.Level.Error).Should().BeFalse();

        var sourceObject = new object[]
        {
                new { InnerObject = 12345 },
                new { InnerObject = 123456789 },
                new { InnerObject1 = 1234567 }
        };

        validater.Invoking(instance => isValidInput = instance.PerformValidation(sourceObject, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        isValidInput.Should().BeFalse();
        events = log4netMemory.GetEvents().Where(ev => ev.Level == log4net.Core.Level.Error).ToArray();
        events.Length.Should().Be(3);
        events.Any(ev => ev.RenderedMessage.StartsWith("The InnerObject field is expected to match the pattern \\d{6,8} but the field value '12345' fails the test")).Should().BeTrue();
        events.Any(ev => ev.RenderedMessage.StartsWith("The InnerObject field is expected to match the pattern \\d{6,8} but the field value '123456789' fails the test")).Should().BeTrue();
        events.Any(ev => ev.RenderedMessage.StartsWith("The InnerObject field is required but an object was passed with the field missing")).Should().BeTrue();
    }

    [Test]
    public void ExpressionValidation()
    {
        DateTime minimumDate = new(2021, 01, 01);
        RuleSetCollection<ValidationRule> validationRuleSets = new();
        validationRuleSets.InitialiseRuleSet("unit_test");
        validationRuleSets["unit_test"].Add("test", new()
        {
            new()
            {
                Expression = (data, log) =>
                {
                    DateTime dateTime = default;
                    int days = default;

                    if (data["date"] is null && data["daysAgo"] is null)
                    {
                        return true;
                    }

                    if (data["daysAgo"] is null && data["date"] is not null && DateTime.TryParse(data["date"].ToString(), out dateTime))
                    {
                        return dateTime > minimumDate;
                    }

                    if (data["date"] is null && data["daysAgo"] is not null && int.TryParse(data["daysAgo"].ToString(), out days))
                    {
                        return DateTime.Now.AddDays(-days) > minimumDate;
                    }

                    log.LogError("An object with both date and daysAgo properties has been provided. Once of the fields need to be removed");
                    return false;
                }
            },
            new()
            {
                Field = "teenage",
                Expression = (data, log) => 13 <= data && data <= 19
            }
        });

        var source = new object[] {
                new {
                    date = DateTime.Now,
                    teenage = 15
                },
                new {
                    daysAgo = 20
                }
            };

        bool isValidInput = false;
        Validate validater = new() { Logger = integrationLogger, RuleSets = validationRuleSets };
        validater.Invoking(instance => isValidInput = instance.PerformValidation(source, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        isValidInput.Should().BeTrue();
        var events = log4netMemory.GetEvents();
        events.Any(ev => ev.Level == log4net.Core.Level.Error).Should().BeFalse();
        log4netMemory.Clear();

        source = new object[] {
                new {
                    teenage = 15
                },
                new {
                    someField = "Pass all tests"
                }
            };

        isValidInput = false;
        validater.Invoking(instance => isValidInput = instance.PerformValidation(source, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        isValidInput.Should().BeTrue();
        events = log4netMemory.GetEvents();
        events.Any(ev => ev.Level == log4net.Core.Level.Error).Should().BeFalse();
        log4netMemory.Clear();

        source = new object[] {
                new {
                    teenage = 60
                },
                new {
                    date = DateTime.Now,
                    daysAgo = 0
                },
                new
                {
                    teenage = 10
                }
            };

        validater.Invoking(instance => isValidInput = instance.PerformValidation(source, "unit_test"))
            .Should().NotThrow<ValidationRuleException>();

        isValidInput.Should().BeFalse();

        events = log4netMemory.GetEvents().Where(ev => ev.Level == log4net.Core.Level.Error).ToArray();
        events.Length.Should().Be(4);
        events.Where(ev => ev.RenderedMessage.StartsWith("The teenage field has failed the evaluation of the validation rule.")).Count().Should().Be(2);
        events.Where(ev => ev.RenderedMessage.StartsWith("An evaluation expression has failed to validate the input.")).Count().Should().Be(1);
        events.Where(ev => ev.RenderedMessage.StartsWith("An object with both date and daysAgo properties has been provided. Once of the fields need to be removed")).Count().Should().Be(1);
    }
}
