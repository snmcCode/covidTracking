using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Common.Models;
using Common.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using Admin.Models;

namespace Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        [BindProperty(SupportsGet = true)]
        [Required]
        public OrganizationModel Organization { get; set; }
        
        public IActionResult OnPost()
        {
            // TODO: check if organization name and password are valid
            return RedirectToPage("/Home/Home");
        }
    }
}