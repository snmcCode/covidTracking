using System;
namespace BackEnd.Models
{
    public class Visit
    {
        public string id;

        public string PartitionKey;

        public Guid VisitorId;

        public string OrganizationId;

        public string Date;

        public string Time;

        public string Door;

        public string Direction;

        private DateTime DateTime;

        public void GenerateId()
        {
            if (VisitorId != null && OrganizationId != null && Date != null && Time != null && Door != null && Direction != null)
            {
                id = $"{VisitorId}_{OrganizationId}_{Date}_{Time}_{Door}_{Direction}";
                PartitionKey = $"{OrganizationId}_{Date}";
            }
        }

        public void GenerateDateTime()
        {
            if (Date == null && Time == null)
            {
                DateTime = DateTime.Now;
                Date = DateTime.ToString("yyyy-MM-dd");
                Time = DateTime.ToString("HH:mm:ss");
            }
        }
    }
}
