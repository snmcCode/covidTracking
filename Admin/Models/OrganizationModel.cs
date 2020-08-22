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
    public class OrganizationModel : Common.Models.Organization
    {
        [BindProperty]
        public new int Id { get; set; }

        [BindProperty]
        public new string Name { get; set; }
    }
}
