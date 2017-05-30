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
        public int id { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public Boolean IsSelected { get; set; }
        public DateTime modifiedDateTime { get; set; }
    }
}