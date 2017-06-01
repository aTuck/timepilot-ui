using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace TimePilot.Entities.Project
{
    public class Project
    {
        public string ProjectKey { get; set; }
        public string Summary { get; set; }
        public bool IsSelected { get; set; }
        public DateTime modifiedDateTime { get; set; }
    }
}