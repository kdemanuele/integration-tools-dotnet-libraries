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

namespace IntegrationTool.Transform.UnitTest;

public class TransformAsIsTests
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

    /// <summary>
    /// Copying string data from one field to another is straightforward. Thus only that the transformation logic throws no exceptions is checked.
    /// </summary>
    [Test]
    public void CopyAsIsStringValue()
    {
        RuleSetCollection<TransformationRule> ruleSetCollection = new();
        ruleSetCollection.InitialiseRuleSet("AsIs");
        ruleSetCollection["AsIs"].TryAdd("Testing", new()
        {
            new() { SourceField = "data", TargetField = "target_data" }
        });

        var source = new
        {
            data = "This is a test string"
        };

        dynamic targetData = null;
        Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };
        Action action = () =>
        {
            targetData = transform.PerformTransformation(source, "AsIs");
        };

        action.Should().NotThrow();

        Assert.IsNotEmpty(targetData);
        Assert.Contains("Testing", targetData.Keys);
        Assert.IsNotEmpty(targetData["Testing"]);
        Assert.AreEqual(1, targetData["Testing"].Count);

        var targetObject = targetData["Testing"][0];

        dynamic testObject = new ExpandoObject();
        testObject.target_data = "This is a test string";

        Assert.AreEqual(testObject, targetObject);
    }

    /// <summary>
    /// Numeric fields can be of a quite a few types. However in .NET the biggest range of number can be stored in 2 types: long and double. For this purpose the transformation threats all numeric fields as either long or double.
    /// 
    /// Although the smaller values can be stored in long or double and given that the source system passes valid values there is no concern that this assumption might cause issues.
    /// </summary>
    [Test]
    public void CopyAsIsNumericValue()
    {
        RuleSetCollection<TransformationRule> ruleSetCollection = new();
        ruleSetCollection.InitialiseRuleSet("AsIs");
        ruleSetCollection["AsIs"].TryAdd("Testing", new()
        {
            new() { SourceField = "data1", TargetField = "int_data" },
            new() { SourceField = "data2", TargetField = "float_data" },
            new() { SourceField = "data3", TargetField = "decimal_data" }
        });

        var source = new
        {
            data1 = 34234,
            data2 = 123.231f,
            data3 = Decimal.MaxValue
        };

        dynamic targetData = null;
        Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };
        Action action = () =>
        {
            targetData = transform.PerformTransformation(source, "AsIs");
        };

        action.Should().NotThrow();

        Assert.IsNotEmpty(targetData);
        Assert.Contains("Testing", targetData.Keys);
        Assert.IsNotEmpty(targetData["Testing"]);
        Assert.AreEqual(1, targetData["Testing"].Count);

        var targetObject = targetData["Testing"][0];
        Assert.IsInstanceOf<long>(targetObject.int_data);
        Assert.AreEqual(source.data1, targetObject.int_data);
        Assert.IsNotInstanceOf<string>(targetObject.int_data);

        Assert.IsInstanceOf<double>(targetObject.float_data);
        Assert.AreEqual(123.231, targetObject.float_data);
        Assert.IsNotInstanceOf<string>(targetObject.float_data);

        Assert.IsInstanceOf<double>(targetObject.decimal_data);
        Assert.AreEqual(Decimal.MaxValue, targetObject.decimal_data);
        Assert.IsNotInstanceOf<string>(targetObject.decimal_data);
    }

    /// <summary>
    /// Copying an object field to another object field introduces a number of complexities. For example:
    /// <list type="bullet">
    /// <item>Does the transformation defined earlier apply to the fields inside the inner objects?</item>
    /// <item>Does the object need to be serialised as JSON or converted to a special representations?</item>
    /// </list>
    /// 
    /// Due to the number of questions that object fields arise, these need to be handled through the complex transformation handling process. Therefore, this test checks that the simple Leave as Is transformation returns null for object fields instead of throwing an exception.
    /// </summary>
    [Test]
    public void CopyAsIsObjectValue()
    {
        RuleSetCollection<TransformationRule> ruleSetCollection = new();
        ruleSetCollection.InitialiseRuleSet("AsIs");
        ruleSetCollection["AsIs"].TryAdd("Testing", new()
        {
            new() { SourceField = "data", TargetField = "obj_data" }
        });

        var source = new
        {
            data = new
            {
                intField = 1324234,
                dateField = DateTime.UtcNow,
                stringField = "This is a test string",
                objField = new
                {
                    field = "Oh another object"
                }
            }
        };

        dynamic targetData = null;
        Transform transform = new() { Logger = integrationLogger, RuleSets = ruleSetCollection };
        Action action = () =>
        {
            targetData = transform.PerformTransformation(source, "AsIs");
        };

        action.Should().NotThrow();

        Assert.IsNotEmpty(targetData);
        Assert.Contains("Testing", targetData.Keys);
        Assert.IsNull(targetData["Testing"].GetType().GetProperty("data"));
    }
}
