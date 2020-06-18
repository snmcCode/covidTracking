using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace MasjidTracker.FrontEnd.Models
{
    public enum Organization
    {
        SNMC,
        Online
    }
    public enum Gender
    {        
        Female,
        Male
    }
    public class Visitor
    {
        public Guid? Id { get; set; }

        [DisplayName("Organization")]
        public Organization RegistrationOrg { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [DisplayName("Email Address")]
        public string Email { get; set; }
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public Guid? FamilyId { get; set; }
        [DisplayName("Gender")]
        public bool IsMale {
            get
            {
                return this.Gender == Gender.Male;
            }
            set
            {
                value = this.Gender == Gender.Male;
            }
        }
        [NotMapped]
        public Gender Gender { get; set; }

        [NotMapped]
        public Byte[] QrCode { get; set; }

    }
}
