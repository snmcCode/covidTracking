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
            string gender = null;

            if (Visitor.IsMale != null)
            {
                gender = Visitor.IsMale == true ? "Male" : "Female";
            }

            VisitorInfo = new VisitorInfo
            {
                id = VisitorInfoId,
                PartitionKey = PartitionKey,
                FirstName = Visitor.FirstName,
                LastName = Visitor.LastName,
                PhoneNumber = Visitor.PhoneNumber,
                Gender = gender ?? "",
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
            if (Organization != null)
            {
                Organization = Organization.Trim();
            }
            if (Door != null)
            {
                Door = Door.Trim();
            }
            if (Direction != null)
            {
                Direction = Direction.Trim();
            }

            // Clean Visitor Fields
            if (Visitor != null)
            {
                if (Visitor.FirstName != null)
                {
                    Visitor.FirstName = Visitor.FirstName.Trim();
                }
                if (Visitor.LastName != null)
                {
                    Visitor.LastName = Visitor.LastName.Trim();
                }
                if (Visitor.Email != null)
                {
                    Visitor.Email = Visitor.Email.Trim();
                }
                if (Visitor.PhoneNumber != null)
                {
                    Visitor.PhoneNumber = Visitor.PhoneNumber.Trim();
                }
            }

            // Clean VisitInfo Fields
            if (VisitInfo != null)
            {
                if (VisitInfo.Organization != null)
                {
                    VisitInfo.Organization = VisitInfo.Organization.Trim();
                }
                if (VisitInfo.Door != null)
                {
                    VisitInfo.Door = VisitInfo.Door.Trim();
                }
                if (VisitInfo.Direction != null)
                {
                    VisitInfo.Direction = VisitInfo.Direction.Trim();

                }
            }


            // Clean VisitorInfo Fields
            if (VisitorInfo != null)
            {
                if (VisitorInfo.FirstName != null)
                {
                    VisitorInfo.FirstName = VisitorInfo.FirstName.Trim();
                }
                if (VisitorInfo.LastName != null)
                {
                    VisitorInfo.LastName = VisitorInfo.LastName.Trim();
                }
                if (VisitorInfo.PhoneNumber != null)
                {
                    VisitorInfo.PhoneNumber = VisitorInfo.PhoneNumber.Trim();
                }
            }
        }
    }
}
