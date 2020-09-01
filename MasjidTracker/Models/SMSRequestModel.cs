using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontEnd.Models
{
    public class SMSRequestModel
    {
        public string Id { get; set; }

        public string PhoneNumber { get; set; }

        public string VerificationCode { get; set; }

    }
}
