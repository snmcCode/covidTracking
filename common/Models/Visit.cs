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
                Date = Date,
                Time = Time
            };

            return VisitorInfo;
        }
    }
}
