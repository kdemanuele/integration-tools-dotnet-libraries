using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using NUnit.Framework;
using System;
using System.Linq;

namespace IntegrationTool.Core.UnitTest;

class IntegrationLoggerLog4NetTests
{
    private IntegrationLogger integrationLogger;
    private MemoryAppender log4netMemory;

    [SetUp]
    public void SetLog4Net()
    {
        log4netMemory = new MemoryAppender();
        BasicConfigurator.Configure(log4netMemory);
        ILog log = LogManager.GetLogger(typeof(IntegrationLoggerLog4NetTests));
        integrationLogger = new(log);
    }

    [Test]
    public void DebugMessageWrittenToLog4Net()
    {
        Action action = () => integrationLogger.LogDebug("This is a test");

        action.Should().NotThrow("No error should be thrown as a logger is attached");

        var events = log4netMemory.GetEvents();
        events.Length.Should().Be(1);
        events.FirstOrDefault().Should().NotBeNull();
        events.FirstOrDefault().Level.Should().Be(Level.Debug);
        // File Line Number is stack is returned as zero in Unit Tests
        events.FirstOrDefault().RenderedMessage.Should().Be("This is a test - IntegrationTool.Core.UnitTest.IntegrationLoggerLog4NetTests:0");
    }

    [Test]
    public void InfoMessageWrittenToLog4Net()
    {
        Action action = () => integrationLogger.LogInfo("This is a test");

        action.Should().NotThrow("No error should be thrown as a logger is attached");

        var events = log4netMemory.GetEvents();
        events.Length.Should().Be(1);
        events.FirstOrDefault().Should().NotBeNull();
        events.FirstOrDefault().Level.Should().Be(Level.Info);
        // File Line Number is stack is returned as zero in Unit Tests
        events.FirstOrDefault().RenderedMessage.Should().Be("This is a test - IntegrationTool.Core.UnitTest.IntegrationLoggerLog4NetTests:0");
    }

    [Test]
    public void TraceMessageWrittenToLog4Net()
    {
        Action action = () => integrationLogger.LogTrace("This is a test");

        action.Should().NotThrow("No error should be thrown as a logger is attached");

        var events = log4netMemory.GetEvents();
        events.Length.Should().Be(1);
        events.FirstOrDefault().Should().NotBeNull();
        events.FirstOrDefault().Level.Should().Be(Level.Info);
        // File Line Number is stack is returned as zero in Unit Tests
        events.FirstOrDefault().RenderedMessage.Should().Be("This is a test - IntegrationTool.Core.UnitTest.IntegrationLoggerLog4NetTests:0");
    }

    [Test]
    public void WarningMessageWrittenToLog4Net()
    {
        Action action = () => integrationLogger.LogWarning("This is a test", null);

        action.Should().NotThrow("No error should be thrown as a logger is attached");

        var events = log4netMemory.GetEvents();
        events.Length.Should().Be(1);
        events.FirstOrDefault().Should().NotBeNull();
        events.FirstOrDefault().Level.Should().Be(Level.Warn);
        // File Line Number is stack is returned as zero in Unit Tests
        events.FirstOrDefault().RenderedMessage.Should().Be("This is a test - IntegrationTool.Core.UnitTest.IntegrationLoggerLog4NetTests:0");
    }

    [Test]
    public void ErrorMessageWrittenToLog4Net()
    {
        Action action = () => integrationLogger.LogError("This is a test", new Exception("Test Error"));

        action.Should().NotThrow("No error should be thrown as a logger is attached");

        var events = log4netMemory.GetEvents();
        events.Length.Should().Be(1);
        events.FirstOrDefault().Should().NotBeNull();
        events.FirstOrDefault().Level.Should().Be(Level.Error);
        // File Line Number is stack is returned as zero in Unit Tests
        events.FirstOrDefault().RenderedMessage.Should().Be("This is a test - IntegrationTool.Core.UnitTest.IntegrationLoggerLog4NetTests:0");
    }
}
