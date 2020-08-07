using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;
using System.ComponentModel;

namespace Admin.Models
{
    public class OrganizationModel : OrganizationCredentialInfo
    {
        [BindProperty]
        [Required(ErrorMessage = "You need to enter your organization ID")]
        public new string LoginName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "You need to enter your password")]
        [PasswordPropertyText]
        public new string LoginSecretHash { get; set; }
    }
}
