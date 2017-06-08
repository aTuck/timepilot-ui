using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimePilot.Controllers;
using TimePilot.Web.Models;
using TimePilot.Entities;

namespace TimePilot.Web.ViewModels
{
    public class ProjectViewModel
    {
        public string SelectedProject { get; set; }
        public List<Project> ProjectList { get; set; }
    }
}