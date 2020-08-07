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

    public class VisitorModel : Visitor
    {
        public new Guid? Id { get; set; }

        [Required]
        [DisplayName("Signed up via")]
        public new Organization RegistrationOrg { get; set; }

        [Required]
        [DisplayName("First Name")]
        public new string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public new string LastName { get; set; }

        [Required]
        [EmailAddress]
        [DisplayName("Email Address")]
        public new string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(10)]
        [DisplayName("Mobile Phone Number")]
        public new string PhoneNumber { get; set; }

        [Required]
        public new bool IsVerified { get; set; }

        [NotMapped]
        public Gender Gender { get; set; }

        [Required]
        public new bool IsMale
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

        public bool isTrue => true;

        [NotMapped]
        [Required]
        [Compare(nameof(isTrue), ErrorMessage = "You must agree to the rules and privacy policy")]
        public bool agreeCheckbox { get; set; }
    }
}
