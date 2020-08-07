using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Home
{
    public class HomeModel : PageModel
    {

        public Visitor visitor;

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

        public IActionResult OnPostRegister()
        {
            return RedirectToPage("Home");
        }
    }
}