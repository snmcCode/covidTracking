using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MasjidTracker.FrontEnd.Models
{
    public class Visitor
    {
        public int Id { get; set; }

        [DisplayName("Organization")]
        public int RegistrationOrg { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int FamilyId { get; set; }
        [DisplayName("Gender")]
        public Boolean IsMale { get; set; }

    }
}
