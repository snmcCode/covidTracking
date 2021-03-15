using Microsoft.AspNetCore.Mvc;

namespace Admin.Models
{
    public class OrganizationModel : Common.Models.Organization
    {
        [BindProperty]
        public new int Id { get; set; }

        [BindProperty]
        public new string Name { get; set; }
    }
}
