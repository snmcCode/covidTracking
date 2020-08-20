using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.Models;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Admin.Pages.Home
{
    public class HomeModel : PageModel
    {
        [BindProperty]
        public string Org { get; set; }

        [BindProperty(SupportsGet = true)]
        public VisitorModel visitor { get; set; }

        [BindProperty(SupportsGet = true)]
        public OrganizationModel Organization { get; set; }

        public IActionResult OnGet(OrganizationModel organization)
        {
            if (organization.Name == null)
            {
                return RedirectToPage("../Index");
            }
            else
            {
                Organization.Name = organization.Name;
                Org = Organization.Name;
                return Page();
            }
        }

        public IActionResult OnPostView()
        {
            return RedirectToPage("Home");
        }

        public IActionResult OnPostEdit()
        {
            return RedirectToPage("Home");
        }

        public IActionResult OnPostDelete()
        {
            return RedirectToPage("Home");
        }

        public IActionResult OnPost()
        {
            Organization.Name = "SNMC";
            return RedirectToPage("/Home/Registration", Organization);
        }
    }
}