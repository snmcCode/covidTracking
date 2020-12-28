using System;
using System.Collections.Generic;
namespace Common.Resources
{
    public static class CustomStatusCodes
    {
        private static Dictionary<int, string> StatusCodesDictionary = new Dictionary<int, string>
        {
            { 000, "Placeholder" },
            { 200, "Success" },
            { 214, "Success, duplicate Entry. Already Registered" },
            { 400, "Request Body Could Not Be Parsed" },
            { 402, "Scanned Visitor is Unverified" },
            { 404, "Not Found in SQL Database" },
            { 406, "EVENT_FULL"  },
            { 409, "BOOKED_SAME_GROUP"  },
            { 412, "NOT_BOOKED" },
            { 422, "Request Body is Valid, But is Missing Required Information" },
            { 500, "General Exception Occurred" },
            { 512, "Error Occurred During SQL Database Operation or Connection" },
            { 513, "Error Occurred During NoSQL Database Operation or Connection" },
            { 514, "Error Occurred During Twilio API Operation or Connection" }
        };

        public static int PLACEHOLDER = 000;

        public static int SUCCESS = 200;
        public static int DUPLICATE = 214;

        public static int BADREQUESTBODY = 400;

        public static int UNVERIFIEDVISITOR = 402;

        public static int NOTFOUNDINSQLDATABASE = 404;

        public static int EVENT_FULL = 406;

        public static int BOOKED_SAME_GROUP = 409;

        public static int NOT_BOOKED = 412;

        public static int BADBUTVALIDREQUESTBODY = 422;

        public static int GENERALERROR = 500;

        public static int SQLDATABASEERROR = 512;

        public static int NOSQLDATABASEERROR = 513;

        public static int TWILIOERROR = 514;

        public static string GetStatusCodeDescription(int statusCode)
        {
            return StatusCodesDictionary.GetValueOrDefault(statusCode);
        }
    }
}
