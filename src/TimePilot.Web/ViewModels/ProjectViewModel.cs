using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimePilot.Controllers;
using TimePilot.Web.Models;

namespace TimePilot.Web.ViewModels
{
    public class ProjectViewModel
    {
        public string SelectedProject { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public List<TimePilot.Entities.Project.Project> ProjectList { get; set; }
    }
}