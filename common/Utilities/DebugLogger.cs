using System;
using System.Text;
using Common.Resources;

using Microsoft.Extensions.Logging;

namespace Common.Utilities
{
    public class DebugLogger
    {
        public DebugLogger(ILogger logger, String apiName, String requestType, String route)
        {
            Logger = logger;
            ApiName = apiName;
            RequestType = requestType;
            Route = route;
        }

        private ILogger Logger;

        public String ApiName;

        public String Route;

        public String RequestType;

        public String RequestBody;

        public String UrlParams;

        public int StatusCode = CustomStatusCodes.PLACEHOLDER;

        public String StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(CustomStatusCodes.PLACEHOLDER);

        public Exception InnerException;

        public String InnerExceptionType;

        public Exception OuterException;

        public String OuterExceptionType;

        public bool Success = true;

        public String Description;

        private String ErrorMessage;

        private String WarningMessage;

        private void GenerateErrorMessage()
        {
            StringBuilder stringBuilder = new StringBuilder()
                .AppendLine($"An Error Occurred in {ApiName} called using {Route} API:")
                .AppendLine()
                .AppendLine($"Request Type: {(RequestType != null ? RequestType : "None")}")
                .AppendLine($"URL Params: {(UrlParams != null && UrlParams != "" ? UrlParams : "None")}")
                .AppendLine($"Request Body:{(RequestBody != null && RequestBody != "" ? RequestBody : "None")}")
                .AppendLine()
                .AppendLine($"Description: {(Description != null && Description != "" ? Description : "None")}")
                .AppendLine()
                .AppendLine($"Status Code: {StatusCode}")
                .AppendLine($"Status Code Description: {(StatusCodeDescription != null ? StatusCodeDescription : "None")}")
                .AppendLine()
                .AppendLine($"Inner Exception Type: {(InnerExceptionType != null ? InnerExceptionType : "None")}")
                .AppendLine($"Inner Exception Message: {(InnerException != null && InnerException.Message != null ? InnerException.Message : "None")}")
                .AppendLine($"Inner Exception Source: {(InnerException != null && InnerException.Source != null ? InnerException.Source : "None")}")
                .AppendLine($"Inner Exception TargetSite: {(InnerException != null && InnerException.TargetSite != null ? InnerException.TargetSite.ToString() : "None")}")
                .AppendLine()
                .AppendLine($"Inner Exception StackTrace: {(InnerException != null && InnerException.StackTrace != null ? InnerException.StackTrace : "None")}")
                .AppendLine()
                .AppendLine($"Outer Exception Type: {(OuterExceptionType != null ? OuterExceptionType : "None")}")
                .AppendLine($"Outer Exception Message: {(OuterException != null && OuterException.Message != null ? OuterException.Message : "None")}")
                .AppendLine($"Outer Exception Source: {(OuterException != null && OuterException.Source != null ? OuterException.Source : "None")}")
                .AppendLine($"Outer Exception TargetSite: {(OuterException != null && OuterException.TargetSite != null ? OuterException.TargetSite.ToString() : "None")}")
                .AppendLine()
                .AppendLine($"Outer Exception StackTrace: {(OuterException != null && OuterException.StackTrace != null ? OuterException.StackTrace : "None")}");

            ErrorMessage = stringBuilder.ToString();
        }

        private void GenerateWarningMessage()
        {
            StringBuilder stringBuilder = new StringBuilder()
                .AppendLine($"A Problem Occurred in {ApiName} called using {Route} API:")
                .AppendLine()
                .AppendLine($"Request Type: {(RequestType != null ? RequestType : "None")}")
                .AppendLine($"URL Params: {(UrlParams != null && UrlParams != "" ? UrlParams : "None")}")
                .AppendLine($"Request Body:{(RequestBody != null && RequestBody != "" ? RequestBody : "None")}")
                .AppendLine()
                .AppendLine($"Description: {(Description != null && Description != "" ? Description : "None")}")
                .AppendLine()
                .AppendLine($"Status Code: {StatusCode}")
                .AppendLine($"Status Code Description: {(StatusCodeDescription != null ? StatusCodeDescription : "None")}")
                .AppendLine()
                .AppendLine($"Inner Exception Type: {(InnerExceptionType != null ? InnerExceptionType : "None")}")
                .AppendLine($"Inner Exception Message: {(InnerException != null && InnerException.Message != null ? InnerException.Message : "None")}")
                .AppendLine($"Inner Exception Source: {(InnerException != null && InnerException.Source != null ? InnerException.Source : "None")}")
                .AppendLine($"Inner Exception TargetSite: {(InnerException != null && InnerException.TargetSite != null ? InnerException.TargetSite.ToString() : "None")}")
                .AppendLine()
                .AppendLine($"Inner Exception StackTrace: {(InnerException != null && InnerException.StackTrace != null ? InnerException.StackTrace : "None")}")
                .AppendLine()
                .AppendLine($"Outer Exception Type: {(OuterExceptionType != null ? OuterExceptionType : "None")}")
                .AppendLine($"Outer Exception Message: {(OuterException != null && OuterException.Message != null ? OuterException.Message : "None")}")
                .AppendLine($"Outer Exception Source: {(OuterException != null && OuterException.Source != null ? OuterException.Source : "None")}")
                .AppendLine($"Outer Exception TargetSite: {(OuterException != null && OuterException.TargetSite != null ? OuterException.TargetSite.ToString() : "None")}")
                .AppendLine()
                .AppendLine($"Outer Exception StackTrace: {(OuterException != null && OuterException.StackTrace != null ? OuterException.StackTrace : "None")}");

            WarningMessage = stringBuilder.ToString();
        }

        public void LogInvocation()
        {
            Logger.LogInformation($"{ApiName} Invoked on URL: {Route}");
        }

        public void LogUrlParams()
        {
            Logger.LogInformation($"URL Parameters: {(UrlParams != null ? UrlParams : "None")}");
        }

        public void LogRequestBody()
        {
            Logger.LogInformation($"Request Body: {(RequestBody != null ? RequestBody : "None")}");
        }

        public void LogSuccess()
        {
            if (Success)
            {
                Logger.LogInformation($"Successfully Invoked {ApiName}");
            }
        }

        public void LogFailure()
        {
            if (!Success)
            {
                StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(StatusCode);
                GenerateErrorMessage();

                Logger.LogError(ErrorMessage);
            }
        }

        public void LogWarning()
        {
            if (!Success)
            {
                StatusCodeDescription = CustomStatusCodes.GetStatusCodeDescription(StatusCode);
                GenerateWarningMessage();

                Logger.LogWarning(WarningMessage);
            }
        }

        public void LogCustomCritical(string message)
        {
            Logger.LogCritical(message);
        }

        public void LogCustomDebug(string message)
        {
            Logger.LogDebug(message);
        }

        public void LogCustomError(string message)
        {
            Logger.LogError(message);
        }

        public void LogCustomInformation(string message)
        {
            Logger.LogInformation(message);
        }

        public void LogCustomMetric(string name, double value)
        {
            Logger.LogMetric(name, value);
        }

        public void LogCustomTrace(string message)
        {
            Logger.LogTrace(message);
        }

        public void LogWarning(string message)
        {
            Logger.LogWarning(message);
        }
    }
}
