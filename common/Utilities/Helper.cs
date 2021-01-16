using System;

using Microsoft.Extensions.Logging;

namespace Common.Utilities
{
    public class LoggerHelper
    {
      
        public LoggerHelper(ILogger logger, String apiName, String requestType, String route)
        {
            DebugLogger = new DebugLogger(logger, apiName, requestType, "/api/" + route);
        }

        public DebugLogger DebugLogger;
    }
}
