using System;
namespace Common.Models
{
    public class Visitor
    {
        public Guid Id;

        public int RegistrationOrg;

        public string FirstName;

        public string LastName;

        public string Email;

        public string PhoneNumber;

        public string Address;

        public Guid FamilyID;

        public bool? IsMale;

        public bool? IsVerified;

        public string LastInfectionDate;

        public string VisitorIdShort;

        public DateTime registrationTime;
    }
}
