using System;
namespace Common.Models
{
    public class VisitInfo
    {
        public string id;

        public string PartitionKey;

        // Links VisitInfo and VisitorInfo
        public string VisitorInfoId;

        public string Organization;

        public string DateTime;

        public string Date;

        public string Time;

        public string Door;

        public string Direction;

        public int? EventId;

        public bool? BookingOverride;

        public bool? CapacityOverride;

        public bool? Offline;

        public string Gender;

        public string ScannerVersion;

        public string DeviceId;

        public string DeviceLocation;

        public string DocType = "Visit";
    }
}
