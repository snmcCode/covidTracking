using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Admin.Models;
using Microsoft.AspNetCore.Authorization;

namespace Admin.Pages
{
    [AllowAnonymous]
    public class ScannerAppModel : PageModel
    {
        private readonly ILogger<ScannerAppModel> _logger;
        private readonly IConfiguration _config;
        private readonly string _targetResource;

        public ScannerAppModel(ILogger<ScannerAppModel> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _targetResource = config["TargetAPIAzureADAPP"];
        }

        public IActionResult OnGet(){
            return Page();
        }

        public IActionResult OnGetScannerApp()
        {
            return RedirectToPage("ScannerApp");
        }
    }
}
