using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TimePilot.Models
{
    public class ProjectViewModel
    {
        //database
        //grabbed using AutoMapper from ProjectRepository
        public int id { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime modifiedDateTime { get; set; }

        //object
        public string Key { get; set; }
        [Display(Name = "Projects")]
        public string Name { get; set; }
        public Boolean IsSelected { get; set; }  

    }

}