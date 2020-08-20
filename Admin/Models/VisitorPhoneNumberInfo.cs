using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.Models
{
    public class VisitorPhoneNumberInfo
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsValidPhoneNumber { get; set; }
        public string PhoneNumberType { get; set; }
        public string VerificationCode { get; set; }
        public string VerificationStatus { get; set; }
    }
}

