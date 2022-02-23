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
using Newtonsoft.Json;

namespace IntegrationTool.Transform.UnitTest;

public class TransformConversionTests
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
    public void ConvertValuesToDouble()
    {
        RuleSetCollection<TransformationRule> ruleSetCollection = new RuleSetCollection<TransformationRule>().InitialiseRuleSet("test");

        ruleSetCollection["test"].Add("unittest", new()
        {
            new TransformationRule { SourceField = "sourceField", TargetField = "targetField", AsType = ITransformationRule.As.Double }
        });

        Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };

        var source = new
        {
            sourceField = "12.13"
        };

        dynamic targetData = null;
        Action action = () => targetData = transform.PerformTransformation(source, "test");

        action.Should().NotThrow();
        var targetDictionary = (IDictionary<string, object>)targetData["unittest"][0];
        targetDictionary.Should().NotBeEmpty();
        targetDictionary.Should().ContainKey("targetField");
        Assert.IsInstanceOf<double>(targetData["unittest"][0].targetField);
        ((double)targetData["unittest"][0].targetField).Should().Be(12.13);

        source = new
        {
            sourceField = "12"
        };

        action.Should().NotThrow();
        targetDictionary = (IDictionary<string, object>)targetData["unittest"][0];
        targetDictionary.Should().NotBeEmpty();
        targetDictionary.Should().ContainKey("targetField");
        Assert.IsInstanceOf<double>(targetData["unittest"][0].targetField);
        ((double)targetData["unittest"][0].targetField).Should().Be(12);

        source = new
        {
            sourceField = "abc"
        };

        action.Should().NotThrow();
        Assert.IsEmpty(targetData["unittest"]);
    }

    [Test]
    public void ConvertValuesToInt()
    {
        RuleSetCollection<TransformationRule> ruleSetCollection = new RuleSetCollection<TransformationRule>().InitialiseRuleSet("test");

        ruleSetCollection["test"].Add("unittest", new()
        {
            new TransformationRule { SourceField = "sourceField", TargetField = "targetField", AsType = ITransformationRule.As.WholeNumber }
        });

        Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };

        var source = new
        {
            sourceField = "12"
        };

        dynamic targetData = null;
        Action action = () => targetData = transform.PerformTransformation(source, "test");

        action.Should().NotThrow();
        var targetDictionary = (IDictionary<string, object>)targetData["unittest"][0];
        targetDictionary.Should().NotBeEmpty();
        targetDictionary.Should().ContainKey("targetField");
        Assert.IsInstanceOf<long>(targetData["unittest"][0].targetField);
        ((int)targetData["unittest"][0].targetField).Should().Be(12);

        source = new
        {
            sourceField = long.MaxValue.ToString()
        };

        action.Should().NotThrow();
        targetDictionary = (IDictionary<string, object>)targetData["unittest"][0];
        targetDictionary.Should().NotBeEmpty();
        targetDictionary.Should().ContainKey("targetField");
        Assert.IsInstanceOf<long>(targetData["unittest"][0].targetField);
        ((double)targetData["unittest"][0].targetField).Should().Be(long.MaxValue);

        source = new
        {
            sourceField = "23.32"
        };

        action.Should().NotThrow();
        Assert.IsEmpty(targetData["unittest"]);

        source = new
        {
            sourceField = "abc"
        };

        action.Should().NotThrow();
        Assert.IsEmpty(targetData["unittest"]);
    }

    [Test]
    public void ConvertValuesToString()
    {
        RuleSetCollection<TransformationRule> ruleSetCollection = new RuleSetCollection<TransformationRule>().InitialiseRuleSet("test");

        ruleSetCollection["test"].Add("unittest", new()
        {
            new TransformationRule { SourceField = "sourceField1", TargetField = "targetField1", AsType = ITransformationRule.As.String },
            new TransformationRule { SourceField = "sourceField2", TargetField = "targetField2", AsType = ITransformationRule.As.String },
            new TransformationRule { SourceField = "sourceField3", TargetField = "targetField3", AsType = ITransformationRule.As.String },
            new TransformationRule { SourceField = "sourceField4", TargetField = "targetField4", AsType = ITransformationRule.As.String },
            new TransformationRule { SourceField = "sourceField5", TargetField = "targetField5", AsType = ITransformationRule.As.String },
            new TransformationRule { SourceField = "sourceField6", TargetField = "targetField6", AsType = ITransformationRule.As.String }
        });

        Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };

        var source = new
        {
            sourceField1 = 12.21412341,
            sourceField2 = 3368946346,
            sourceField3 = DateTime.UtcNow,
            sourceField4 = TimeSpan.FromDays(23),
            sourceField5 = new { Field = 12 },
            sourceField6 = "This should be the same"
        };

        dynamic targetData = null;
        Action action = () => targetData = transform.PerformTransformation(source, "test");

        action.Should().NotThrow();

        var targetObject = targetData["unittest"][0];
        Assert.IsInstanceOf<string>(targetObject.targetField1);
        Assert.AreEqual("12.21412341", targetObject.targetField1);

        Assert.IsInstanceOf<string>(targetObject.targetField2);
        Assert.AreEqual("3368946346", targetObject.targetField2);

        Assert.IsInstanceOf<string>(targetObject.targetField3);
        Assert.AreEqual(source.sourceField3.ToString(), targetObject.targetField3);

        Assert.IsInstanceOf<string>(targetObject.targetField4);
        Assert.AreEqual(TimeSpan.FromDays(23).ToString(), targetObject.targetField4);

        Assert.IsInstanceOf<string>(targetObject.targetField5);
        Assert.AreEqual($"{{{Environment.NewLine}  \"Field\": 12{Environment.NewLine}}}", targetObject.targetField5);

        Assert.IsInstanceOf<string>(targetObject.targetField6);
        Assert.AreEqual("This should be the same", targetObject.targetField6);
    }

    [Test]
    public void ConvertValuesToIsoUtcDate()
    {
        RuleSetCollection<TransformationRule> ruleSetCollection = new RuleSetCollection<TransformationRule>().InitialiseRuleSet("test");

        ruleSetCollection["test"].Add("unittest", new()
        {
            new TransformationRule { SourceField = "sourceField1", TargetField = "targetField1", AsType = ITransformationRule.As.IsoUtcDate },
            new TransformationRule { SourceField = "sourceField2", TargetField = "targetField2", AsType = ITransformationRule.As.IsoUtcDate },
            new TransformationRule { SourceField = "sourceField3", TargetField = "targetField3", AsType = ITransformationRule.As.IsoUtcDate },
            new TransformationRule { SourceField = "sourceField4", TargetField = "targetField4", AsType = ITransformationRule.As.IsoUtcDate },
            new TransformationRule { SourceField = "sourceField5", TargetField = "targetField5", AsType = ITransformationRule.As.IsoUtcDate },
            new TransformationRule { SourceField = "sourceField6", TargetField = "targetField6", AsType = ITransformationRule.As.IsoUtcDate }
        });

        Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };

        var source = new
        {
            sourceField1 = "2021/01/01T12:00:00+01:00",
            sourceField2 = "2021/01/01",
            sourceField3 = "2021/01/01T12:00:00Z",
            sourceField4 = "1 January 2021",
            sourceField5 = "1-1-2021",
            sourceField6 = "Not a Date"
        };

        var stringDate = new DateTime(2021, 01, 01).ToUniversalTime().ToString("O");

        dynamic targetData = null;
        Action action = () => targetData = transform.PerformTransformation(source, "test");

        action.Should().NotThrow();

        var targetObject = targetData["unittest"][0];
        Assert.IsInstanceOf<string>(targetObject.targetField1);
        Assert.AreEqual(new DateTime(2021, 01, 01, 11, 0, 0, DateTimeKind.Utc).ToString("O"), targetObject.targetField1);

        Assert.IsInstanceOf<string>(targetObject.targetField2);
        Assert.AreEqual(stringDate, targetObject.targetField2);

        Assert.IsInstanceOf<string>(targetObject.targetField3);
        Assert.AreEqual(new DateTime(2021, 01, 01, 12, 0, 0, DateTimeKind.Utc).ToString("O"), targetObject.targetField3);

        Assert.IsInstanceOf<string>(targetObject.targetField4);
        Assert.AreEqual(stringDate, targetObject.targetField4);

        Assert.IsInstanceOf<string>(targetObject.targetField5);
        Assert.AreEqual(stringDate, targetObject.targetField5);

        var targetDictionary = (IDictionary<string, object>)targetObject;
        targetDictionary.Should().NotContainKey("targetField6");
    }


    [Test]
    public void ConvertValuesToIsoDate()
    {
        RuleSetCollection<TransformationRule> ruleSetCollection = new RuleSetCollection<TransformationRule>().InitialiseRuleSet("test");

        ruleSetCollection["test"].Add("unittest", new()
        {
            new TransformationRule { SourceField = "sourceField1", TargetField = "targetField1", AsType = ITransformationRule.As.IsoDate },
            new TransformationRule { SourceField = "sourceField2", TargetField = "targetField2", AsType = ITransformationRule.As.IsoDate },
            new TransformationRule { SourceField = "sourceField3", TargetField = "targetField3", AsType = ITransformationRule.As.IsoDate },
            new TransformationRule { SourceField = "sourceField4", TargetField = "targetField4", AsType = ITransformationRule.As.IsoDate },
            new TransformationRule { SourceField = "sourceField5", TargetField = "targetField5", AsType = ITransformationRule.As.IsoDate },
            new TransformationRule { SourceField = "sourceField6", TargetField = "targetField6", AsType = ITransformationRule.As.IsoDate }
        });

        Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };

        var source = new
        {
            sourceField1 = "2021/01/01T12:00:00+09:00",
            sourceField2 = "2021/01/01",
            sourceField3 = "2021/01/01T12:00:00Z",
            sourceField4 = "1 January 2021",
            sourceField5 = "1-1-2021",
            sourceField6 = "Not a Date"
        };

        dynamic targetData = null;
        Action action = () => targetData = transform.PerformTransformation(source, "test");

        var stringDate = new DateTime(2021, 01, 01).ToString("O");

        action.Should().NotThrow();

        var gmtPlus9 = TimeZoneInfo.CreateCustomTimeZone("GMT+9", TimeSpan.FromHours(9), "GMT +9", "GMT +9");

        var targetObject = targetData["unittest"][0];
        Assert.IsInstanceOf<string>(targetObject.targetField1);
        Assert.AreEqual(TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2021, 01, 01, 12, 0, 0), gmtPlus9), TimeZoneInfo.Local).ToString("yyyy-MM-ddTHH:mm:ss.fffffff%K"), targetObject.targetField1);

        Assert.IsInstanceOf<string>(targetObject.targetField2);
        Assert.AreEqual(stringDate, targetObject.targetField2);

        Assert.IsInstanceOf<string>(targetObject.targetField3);
        Assert.AreEqual(TimeZoneInfo.ConvertTime(new DateTime(2021, 01, 01, 12, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Local).ToString("yyyy-MM-ddTHH:mm:ss.fffffff%K"), targetObject.targetField3);

        Assert.IsInstanceOf<string>(targetObject.targetField4);
        Assert.AreEqual(stringDate, targetObject.targetField4);

        Assert.IsInstanceOf<string>(targetObject.targetField5);
        Assert.AreEqual(stringDate, targetObject.targetField5);

        var targetDictionary = (IDictionary<string, object>)targetObject;
        targetDictionary.Should().NotContainKey("targetField6");
    }

    [Test]
    public void SerializeObjectToJsonRepresentation()
    {
        RuleSetCollection<TransformationRule> ruleSetCollection = new RuleSetCollection<TransformationRule>().InitialiseRuleSet("test");

        ruleSetCollection["test"].Add("unittest", new()
        {
            new TransformationRule { SourceField = "sourceField1", TargetField = "targetField1", AsType = ITransformationRule.As.SerializeJson },
            new TransformationRule { SourceField = "sourceField2", TargetField = "targetField2", AsType = ITransformationRule.As.SerializeJson },
            new TransformationRule { SourceField = "sourceField3", TargetField = "targetField3", AsType = ITransformationRule.As.SerializeJson },
        });

        var source = new
        {
            sourceField1 = new
            {
                intField = 1324234,
                dateField = DateTime.UtcNow,
                stringField = "This is a test string",
                objField = new
                {
                    field = "Oh another object"
                }
            },
            sourceField2 = 12,
            sourceField3 = DateTime.Today
        };

        Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };
        dynamic targetData = null;

        Action action = () => targetData = transform.PerformTransformation(source, "test");

        action.Should().NotThrow();

        var targetDictionary = (IDictionary<string, object>)targetData["unittest"][0];
        targetDictionary.Should().ContainKeys("targetField1", "targetField2", "targetField3");

        var targetObject = targetData["unittest"][0];
        ((string)targetObject.targetField1).Should().Be(JsonConvert.SerializeObject(source.sourceField1));
        ((string)targetObject.targetField2).Should().Be("12");
        ((string)targetObject.targetField3).Should().Be($"\"{DateTime.Today:yyyy-MM-ddT00:00:00%K}\"");
    }
}
