using DotNet.Memory.Logger;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Core.UnitTest;

class IntegrationLoggerMicrosoftLoggerTests
{
    private MemoryLoggerProvider memoryLoggerProvider;
    private IntegrationLogger integrationLogger;

    [SetUp]
    public void SetMicrosoftLogger()
    {
        LoggerFactory factory = new();
        MemoryLoggerSettings memoryLoggerSettings = new();
        memoryLoggerProvider = new MemoryLoggerProvider(memoryLoggerSettings);
        factory.AddProvider(memoryLoggerProvider);

        // Create fake logger
        ILogger memoryLogger = factory.CreateLogger("Fake");

        integrationLogger = new(memoryLogger);
    }

    [Test]
    public void DebugMessageWrittenToLogger()
    {
        Action action = () => integrationLogger.LogDebug("This is a test");

        action.Should().NotThrow("No error should be thrown as a logger is attached");

        memoryLoggerProvider.MemoryLoggers["Fake"].TryGetTarget(out MemoryLogger memoryLogger);
        var events = memoryLogger.LogMessages;
        events.Count.Should().Be(1);
        events.FirstOrDefault().Should().NotBeNull();
        events.FirstOrDefault().LogLevel.Should().Be(LogLevel.Debug);
        // File Line Number is stack is returned as zero in Unit Tests
        events.FirstOrDefault().Message.Should().Be("This is a test - IntegrationTool.Core.UnitTest.IntegrationLoggerMicrosoftLoggerTests:0");
    }

    [Test]
    public void InfoMessageWrittenToLogger()
    {
        Action action = () => integrationLogger.LogInfo("This is a test");

        action.Should().NotThrow("No error should be thrown as a logger is attached");

        memoryLoggerProvider.MemoryLoggers["Fake"].TryGetTarget(out MemoryLogger memoryLogger);
        var events = memoryLogger.LogMessages;
        events.Count.Should().Be(1);
        events.FirstOrDefault().Should().NotBeNull();
        events.FirstOrDefault().LogLevel.Should().Be(LogLevel.Information);
        // File Line Number is stack is returned as zero in Unit Tests
        events.FirstOrDefault().Message.Should().Be("This is a test - IntegrationTool.Core.UnitTest.IntegrationLoggerMicrosoftLoggerTests:0");
    }

    [Test]
    public void TraceMessageWrittenToLogger()
    {
        Action action = () => integrationLogger.LogTrace("This is a test");

        action.Should().NotThrow("No error should be thrown as a logger is attached");

        memoryLoggerProvider.MemoryLoggers["Fake"].TryGetTarget(out MemoryLogger memoryLogger);
        var events = memoryLogger.LogMessages;
        events.Count.Should().Be(1);
        events.FirstOrDefault().Should().NotBeNull();
        events.FirstOrDefault().LogLevel.Should().Be(LogLevel.Trace);
        // File Line Number is stack is returned as zero in Unit Tests
        events.FirstOrDefault().Message.Should().Be("This is a test - IntegrationTool.Core.UnitTest.IntegrationLoggerMicrosoftLoggerTests:0");
    }

    [Test]
    public void WarningMessageWrittenToLogger()
    {
        Action action = () => integrationLogger.LogWarning("This is a test", null);

        action.Should().NotThrow("No error should be thrown as a logger is attached");

        memoryLoggerProvider.MemoryLoggers["Fake"].TryGetTarget(out MemoryLogger memoryLogger);
        var events = memoryLogger.LogMessages;
        events.Count.Should().Be(1);
        events.FirstOrDefault().Should().NotBeNull();
        events.FirstOrDefault().LogLevel.Should().Be(LogLevel.Warning);
        // File Line Number is stack is returned as zero in Unit Tests
        events.FirstOrDefault().Message.Should().Be("This is a test - IntegrationTool.Core.UnitTest.IntegrationLoggerMicrosoftLoggerTests:0");
    }

    [Test]
    public void ErrorMessageWrittenToLogger()
    {
        Action action = () => integrationLogger.LogError("This is a test", new Exception("This is a test"));

        action.Should().NotThrow("No error should be thrown as a logger is attached");

        memoryLoggerProvider.MemoryLoggers["Fake"].TryGetTarget(out MemoryLogger memoryLogger);
        var events = memoryLogger.LogMessages;
        events.Count.Should().Be(1);
        events.FirstOrDefault().Should().NotBeNull();
        events.FirstOrDefault().LogLevel.Should().Be(LogLevel.Error);
        // File Line Number is stack is returned as zero in Unit Tests
        events.FirstOrDefault().Message.Should().Be("This is a test - IntegrationTool.Core.UnitTest.IntegrationLoggerMicrosoftLoggerTests:0");
    }
}
