using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimePilot.Web.Models;

namespace TimePilot.Web.ViewModels
{
    public class StoryViewModel
    {
        public int[] StorypointSum { get; set; }
        public List<Story> mStoryList { get; set; }
    }
}