using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimePilot.Web.ViewModels
{
    public class ResultsViewModel
    {


        public int [] storypointAllocation { get; set; }
        public int [] numberOfStories { get; set; }
        public int [] DaysPerPt { get; set; }
        public int [] Total { get; set; }
        public int TotalDays { get; set; }
        public int TotalHours { get; set; }



    }
}