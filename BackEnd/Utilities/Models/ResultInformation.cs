using System;
namespace BackEnd.Utilities.Models
{
    public class ResultInformation
    {
        public ResultInformation(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
        public int StatusCode;

        public string Message;
    }
}
