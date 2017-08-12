using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimePilot.Entities;

namespace TimePilot.Web.ViewModels
{
    public class ResultsViewModel
    {
        public int[] storyPointAllocation { get; set; }
        public List<Conversion> DaysPerPt { get; set; }
        public float totalDevCapacity { get; set; }
        public float totalStoryPoints { get; set; }
        public int totalNumOfSprints { get; set; }
    }
}