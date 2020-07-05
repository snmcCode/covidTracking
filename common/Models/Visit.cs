using System;
namespace Common.Models
{
    public class Visit
    {
        public string id;

        public string PartitionKey;

        public Guid VisitorId;

        public string VisitorInfoId;

        public string Organization;

        public string Date;

        public string Time;

        public string Door;

        public string Direction;

        public Visitor Visitor;

        private DateTime DateTime;

        private VisitInfo VisitInfo;

        private VisitorInfo VisitorInfo;

        public void GenerateDateTime()
        {
            if (Date == null && Time == null)
            {
                DateTime = DateTime.Now;
                Date = DateTime.ToString("yyyy-MM-dd");
                Time = DateTime.ToString("HH:mm:ss");
            }
        }

        public void GenerateId()
        {
            if (VisitorId != Guid.Empty && Organization != null && Date != null && Time != null && Door != null && Direction != null)
            {
                id = Guid.NewGuid().ToString();
                VisitorInfoId = Guid.NewGuid().ToString();
                PartitionKey = $"{Organization}_{Date}_{Door}";
            }
        }

        public VisitInfo GetVisitInfo()
        {
            VisitInfo = new VisitInfo
            {
                id = id,
                PartitionKey = PartitionKey,
                VisitorInfoId = VisitorInfoId,
                Organization = Organization,
                DateTime = DateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                Date = Date,
                Time = Time,
                Door = Door,
                Direction = Direction
            };

            return VisitInfo;
        }

        public VisitorInfo GetVisitorInfo()
        {
            VisitorInfo = new VisitorInfo
            {
                id = VisitorInfoId,
                PartitionKey = PartitionKey,
                FirstName = Visitor.FirstName,
                LastName = Visitor.LastName,
                PhoneNumber = Visitor.PhoneNumber,
                DateTime = DateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                Date = Date,
                Time = Time
            };

            return VisitorInfo;
        }

        public void FinalizeData()
        {
            CleanData();

            // May Require More Function Calls In Future
        }

        private void CleanData()
        {
            // Clean Fields
            Organization = Organization.Trim();
            Door = Door.Trim();
            Direction = Direction.Trim();

            // Clean Visitor Fields
            Visitor.FirstName = Visitor.FirstName.Trim();
            Visitor.LastName = Visitor.LastName.Trim();
            Visitor.Email = Visitor.Email.Trim();
            Visitor.PhoneNumber = Visitor.PhoneNumber.Trim();

            // Clean VisitInfo Fields
            VisitInfo.Organization = VisitInfo.Organization.Trim();
            VisitInfo.Door = VisitInfo.Door.Trim();
            VisitInfo.Direction = VisitInfo.Direction.Trim();

            // Clean VisitorInfo Fields
            VisitorInfo.FirstName = VisitorInfo.FirstName.Trim();
            VisitorInfo.LastName = VisitorInfo.LastName.Trim();
            VisitorInfo.PhoneNumber = VisitorInfo.PhoneNumber.Trim();
        }
    }
}
