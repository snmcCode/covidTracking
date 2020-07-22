using System;
using Common.Models;

namespace Common.Models
{
    public class ScannerLoginOrganizationInfo
    {
        public ScannerLoginOrganizationInfo(int id, String name, String clientId, String clientSecret)
        {
            Id = id;
            Name = name;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public int Id;

        public String Name;

        public String ClientId;

        public String ClientSecret;
    }
}
