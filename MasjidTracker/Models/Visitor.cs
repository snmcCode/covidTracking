using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace MasjidTracker.FrontEnd.Models
{
    public enum Organization
    {
        Online,
        SNMCs
    }
    public enum Gender
    {        
        Female,
        Male
    }
    public class Visitor
    {
        public Guid? Id { get; set; }

        [DisplayName("Signed up via")]
        public Organization RegistrationOrg { get; set; }

        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [DisplayName("Email Address")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }
        public Guid? FamilyId { get; set; }

        [Required]
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
