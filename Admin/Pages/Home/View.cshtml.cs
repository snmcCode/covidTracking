using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Admin.Models;
using System.Security.Claims;
using Admin.Util;

namespace Admin.Pages.Home
{
    public class ViewModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string printTitle { get; set; }

        [BindProperty]
        public VisitorModel Visitor { get; set; }
        public IActionResult OnGet(VisitorModel visitor)
        {
            if (visitor == null)
            {
                return RedirectToRoute("/Home/Home");
            }
            Visitor = visitor;
            Visitor.QrCode = Utils.GenerateQRCodeBitmapByteArray(Visitor.Id.ToString());

            return Page();
        }

        // Called when The Register Another button is pressed
        public IActionResult OnPostRegisterAnother()
        {
            var id = Int32.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            var name = HttpContext.User.FindFirstValue(ClaimTypes.Name);

            return RedirectToPage("/Home/Registration", new { Id = id, Name = name });
        }

        public IActionResult OnPostLoginAnother()
        {
            return RedirectToPage("/Home/Login");
        }
    }
}