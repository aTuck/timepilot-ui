using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimePilot.Web.ViewModels
{
    public class ResultsViewModel
    {
        public int[] storypointAllocation { get; set; }
        public int[] numberOfStories { get; set; }
        public int[] DaysPerPt { get; set; }
        public int[] Total { get; set; }
        public int TotalDays { get; set; }
        public float TotalHours { get; set; }
        public float TotalWeeks { get; set; }
        public int SprintLength { get; set; }
        public float TotalDevQA { get; set; }
        public float AvgCapacitiyperWeek { get; set; }
        public float ProjectDurationWeeks { get; set; }
        public int ReleaseAndHardening { get; set; }
        public float Contingency { get; set; }
        public float totalStoryPoints { get; set; }
        public float TeamVelocity { get; set; }
        public float TotalSprints { get; set; }
        public float TotalWeeksVelocity { get; set; }
        public float projectDurationWeeksVelocity { get; set; }
        public float totalPointsVelocity { get; set; }
        public IEnumerable<SelectListItem> sprintLengthList {get;set;}
    }
}