using System;

using Microsoft.Extensions.Logging;

namespace BackEnd.Utilities
{
    public class Helper
    {
        public Helper(ILogger logger, String apiName, String requestType, String route)
        {
            DebugLogger = new DebugLogger(logger, apiName, requestType, "/api/" + route);
        }

        public DebugLogger DebugLogger;
    }
}
