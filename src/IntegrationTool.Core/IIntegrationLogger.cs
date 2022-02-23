using System;

namespace IntegrationTool.Core;

public interface IIntegrationLogger
{
    public void LogInfo(string message);
    public void LogTrace(string message);
    public void LogDebug(string message);
    public void LogError(string message, Exception exception = null);
    public void LogWarning(string message, Exception exception = null);
}
