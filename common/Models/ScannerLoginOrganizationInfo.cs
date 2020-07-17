using System;
using Common.Models;

namespace Common.Models
{
    public class ScannerLoginOrganizationInfo
    {
        public ScannerLoginOrganizationInfo(int organizationId, String organizationName, String scannerClientId, String scannerClientSecret)
        {
            OrganizationId = organizationId;
            OrganizationName = organizationName;
            ScannerClientId = scannerClientId;
            ScannerClientSecret = scannerClientSecret;
        }

        public int OrganizationId;

        public String OrganizationName;

        public String ScannerClientId;

        public String ScannerClientSecret;
    }
}
