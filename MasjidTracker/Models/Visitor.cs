using FrontEnd.Models;
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
        SNMC
       
    }
    public enum Gender
    {
        Male,
        Female
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

        
        [EmailAddress]
        [DisplayName("Email Address")]
        public string Email { get; set; }

       
        [Required(ErrorMessage = "Please enter a mobile phone number.")]
        [Phone]
        [StringLength(10)]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "Your 10 digit phone number cannot contain any spaces, dashes, or brackets")]
        [DisplayName("Mobile Phone Number")]
        public string PhoneNumber { get; set; }
        //public string Address { get; set; }

        public bool isTrue => true;

        [NotMapped]
        [Required]
        [Compare(nameof(isTrue), ErrorMessage = "You must agree to the rules and privacy policy")]
        public bool agreeCheckbox {get; set;}

        public Guid? FamilyId { get; set; }

        public bool isVerified { get; set; }

        [NotMapped]
        public Gender Gender { get; set; }

        [Required]
        [DisplayName("Gender")]
        public bool IsMale
        {
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
        public Byte[] QrCode { get; set; }

        [NotMapped]
        public string VerificationCode { get; set; }

        //public SMSRequestModel smsRequestModel { get; set; }

    }
}
