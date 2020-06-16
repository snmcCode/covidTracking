using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasjidTracker.Models
{
    public class Visitor
    {
        public int Id { get; set; }
        public int RegistrationOrg { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int FamilyId { get; set; }
        public Boolean IsMale { get; set; }

    }
}
