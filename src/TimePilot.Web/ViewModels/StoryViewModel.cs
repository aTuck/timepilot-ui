﻿using Newtonsoft.Json;
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
        public int[] StorypointSum { get; set; }

        public List<Story> mStoryList { get; set; }

        public Boolean ZeroStoryFlag { get; set;}

        public int totalNumberStoryPoints { get; set; }
        public List<Story> StoryList { get; set; }
        public bool isSelectedToDelete { get; set; }
    }
}