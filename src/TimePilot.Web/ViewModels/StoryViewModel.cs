using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimePilot.Entities;

namespace TimePilot.Web.ViewModels
{
    public class StoryViewModel
    {
        public int[] StoryPointAllocation { get; set; }
        public int totalStoryPoints { get; set; }
        public List<Story> StoryList { get; set; }
        public Dictionary<string, Epic> EpicList { get; set; }
        public string selectedEpic { get; set; }
    }
}