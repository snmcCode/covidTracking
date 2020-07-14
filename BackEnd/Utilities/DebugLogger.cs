using System;
using System.Text;
using Common.Resources;

using Microsoft.Extensions.Logging;

namespace BackEnd.Utilities
{
    public class DebugLogger
    {
        public DebugLogger(ILogger logger, String apiName, String requestType, String route)
        {
            Logger = logger;
            ApiName = apiName;
            RequestType = requestType;
            Route = "/api/" + route;
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

        private void GenerateErrorMessage()
        {
            StringBuilder stringBuilder = new StringBuilder()
                .AppendLine($"An Error Occurred in {ApiName} called using {Route} API:")
                .AppendLine()
                .AppendLine($"Request Type: {(RequestType != null ? RequestType : "")}")
                .AppendLine($"URL Params: {(UrlParams != null && UrlParams != "" ? UrlParams : "None")}")
                .AppendLine($"Request Body:{(RequestBody != null && RequestBody != "" ? RequestBody : "None")}")
                .AppendLine()
                .AppendLine($"Description: {(Description != null && Description != "" ? Description : "None")}")
                .AppendLine()
                .AppendLine($"Status Code: {StatusCode}")
                .AppendLine($"Status Code Description: {(StatusCodeDescription != null ? StatusCodeDescription : "")}")
                .AppendLine()
                .AppendLine($"Inner Exception Type: {(InnerExceptionType != null ? InnerExceptionType : "")}")
                .AppendLine($"Inner Exception Message: {(InnerException != null && InnerException.Message != null ? InnerException.Message : "")}")
                .AppendLine($"Inner Exception Source: {(InnerException != null && InnerException.Source != null ? InnerException.Source : "")}")
                .AppendLine($"Inner Exception TargetSite: {(InnerException != null && InnerException.TargetSite != null ? InnerException.TargetSite.ToString() : "")}")
                .AppendLine()
                .AppendLine($"Inner Exception StackTrace: {(InnerException != null && InnerException.StackTrace != null ? InnerException.StackTrace : "")}")
                .AppendLine()
                .AppendLine($"Outer Exception Type: {(OuterExceptionType != null ? OuterExceptionType : "")}")
                .AppendLine($"Outer Exception Message: {(OuterException != null && OuterException.Message != null ? OuterException.Message : "")}")
                .AppendLine($"Outer Exception Source: {(OuterException != null && OuterException.Source != null ? OuterException.Source : "")}")
                .AppendLine($"Outer Exception TargetSite: {(OuterException != null && OuterException.TargetSite != null ? OuterException.TargetSite.ToString() : "")}")
                .AppendLine()
                .AppendLine($"Outer Exception StackTrace: {(OuterException != null && OuterException.StackTrace != null ? OuterException.StackTrace : "")}");

            ErrorMessage = stringBuilder.ToString();
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
    }
}
