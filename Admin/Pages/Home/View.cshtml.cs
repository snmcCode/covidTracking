using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Admin.Models;

namespace Admin.Pages.Home
{
    public class ViewModel : PageModel
    {
        [BindProperty]
        public VisitorModel Visitor { get; set; }

        public IActionResult OnGet(VisitorModel visitor)
        {
            if (visitor.FirstName == null || visitor.LastName == null || visitor.PhoneNumber == null || visitor.RegistrationOrg == null)
            {
                return RedirectToPage("../Index");
            }
            else
            {
                Visitor = visitor;
                return Page();
            }
        }
    }
}