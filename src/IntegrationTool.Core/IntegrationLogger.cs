using log4net;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Core
{
    public class IntegrationLogger: IIntegrationLogger
    {
        private readonly ILog log4NetLogger;
        private readonly ILogger microsoftLogger;

        public IntegrationLogger(ILog log)
        {
            log4NetLogger = log;
        }

        public IntegrationLogger(ILogger log)
        {
            microsoftLogger = log;
        }
        
        public void LogDebug(string message)
        {
            var fullMessage = GetMessageWithMethodInfo(message);
            log4NetLogger?.Debug(fullMessage);
            microsoftLogger?.LogDebug(fullMessage);
        }

        public void LogError(string message, Exception exception)
        {
            var fullMessage = GetMessageWithMethodInfo(message);
            log4NetLogger?.Error(fullMessage, exception);
            microsoftLogger?.LogError(exception, fullMessage);
        }

        public void LogInfo(string message)
        {
            var fullMessage = GetMessageWithMethodInfo(message);
            log4NetLogger?.Info(fullMessage);
            microsoftLogger?.LogInformation(fullMessage);
        }

        public void LogTrace(string message)
        {
            var fullMessage = GetMessageWithMethodInfo(message);
            log4NetLogger?.Info(fullMessage);
            microsoftLogger?.LogTrace(fullMessage);
        }

        public void LogWarning(string message, Exception exception)
        {
            var fullMessage = GetMessageWithMethodInfo(message);
            log4NetLogger?.Warn(fullMessage, exception);
            microsoftLogger?.LogWarning(exception, fullMessage);
        }

        private static string GetMessageWithMethodInfo(string message)
        {
            StackTrace stackTrace = new();
            string methodName = stackTrace.GetFrame(2).GetMethod().ReflectedType.FullName;
            int lineNumber = stackTrace.GetFrame(2).GetFileLineNumber();

            return $"{message} - {methodName}:{lineNumber}";
        }
    }
}
