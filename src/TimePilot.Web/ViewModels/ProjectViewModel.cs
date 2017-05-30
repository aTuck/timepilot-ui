using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimePilot.Controllers;
using TimePilot.Models;

namespace TimePilot.ViewModels
{
    public class ProjectViewModel
    {
        public string SelectedProject { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public List<Project> ProjectList { get; set; }
    }
}