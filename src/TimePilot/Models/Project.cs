using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TimePilot.Models
{
    public class Project
    {
        [Display(Name = "Blah")]
        public string Description { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public Boolean IsSelected { get; set; }  

    }

}