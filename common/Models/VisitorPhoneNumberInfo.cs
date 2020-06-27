using System;
namespace Common.Models
{
    public class VisitorPhoneNumberInfo
    {
        public Guid Id;

        public string PhoneNumber;

        public bool IsValidPhoneNumber;

        public string PhoneNumberType;

        public string VerificationCode;

        public string VerificationStatus;
    }
}
