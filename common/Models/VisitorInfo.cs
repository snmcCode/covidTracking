using System;
namespace Common.Models
{
    public class VisitorInfo
    {
        public string id;

        public string PartitionKey;

        public string FirstName;

        public string LastName;

        public string PhoneNumber;

        public string DateTime;

        public string Date;

        public string Time;

        public string ScannerVersion;

        public string DeviceId;

        public string DeviceLocation;

        public string DocType = "Visitor";

        // Set to the number of seconds in 90 days
        public int ttl = 90 * 24 * 60 * 60;
    }
}
