using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;

namespace Admin.Models
{
    public enum Gender
    {
        Male,
        Female
    }

    public class VisitorModel : Visitor
    {
        public new Guid? Id { get; set; }

        [Required]
        [DisplayName("Signed up via")]
        public new string RegistrationOrg { get; set; }

        [Required(ErrorMessage = "Please enter a first name.")]
        [DisplayName("First Name")]
        public new string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter a last name.")]
        [DisplayName("Last Name")]
        public new string LastName { get; set; }

        [DisplayName("Email Address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public new string Email { get; set; }

        [Required(ErrorMessage = "Please enter a mobile phone number.")]
        [Phone]
        [StringLength(10)]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "Your 10 digit phone number cannot contain any spaces, dashes, or brackets")]
        [DisplayName("Mobile Phone Number")]
        public new string PhoneNumber { get; set; }

        [Required]
        public new bool? IsVerified { get; set; }

        [NotMapped]
        [DefaultValue(Gender.Female)]
        public Gender Gender { get; set; }

        [Required]
        public new bool? IsMale
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

        public string visitorIdShort {get; set;}

        public DateTime registrationTime {get; set;}
    }
}
