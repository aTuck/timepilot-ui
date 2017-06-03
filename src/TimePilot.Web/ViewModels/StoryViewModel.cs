using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimePilot.Models;

namespace TimePilot.ViewModels
{
    public class StoryViewModel
    {
        public int[] StorypointSum { get; set; }
        public List<Story> mStoryList { get; set; }

        public Boolean ZeroStoryFlag { get; set;}
    }
}