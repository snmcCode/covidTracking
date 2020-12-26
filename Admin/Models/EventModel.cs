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
    public class EventModel : Common.Models.Organization
    {

        [BindProperty]
        public new int Id { get; set; }

        [BindProperty]
        public int OrgId { get; set; }

        [BindProperty]
        [Display(Name = "Group Id")]
        public String Groupid {get; set;}

        [BindProperty]
        [Required(ErrorMessage = "Please enter an event name.")]
        public new string Name { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter an event date.")]
        public DateTime DateTime {get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter the capacity of the event.")]
        [Range(0, int.MaxValue, ErrorMessage ="Please enter a positive value.")]
        public int Capacity {get; set;}

        [BindProperty]
        [Display(Name = "Private")]
        [Required(ErrorMessage = "Please indicate whether the event is private.")]
        public Boolean IsPrivate {get; set;}

        [BindProperty]
        [Required(ErrorMessage = "Please specify the name of the hall that will host this event.")]
        public string Hall {get; set;}
    }
}
