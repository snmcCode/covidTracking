using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.Pages.Home
{
    public class VerifyVisitorModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public VisitorModel Visitor { get; set; }

        [BindProperty]
        public string VerificationCode { get; set; }

        public void OnGet(VisitorModel visitor)
        {
            Visitor = visitor;
        }
    }
}