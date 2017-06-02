using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimePilot.Entities
{
    public class Story
    {
        public int StoryID { get; set; }
        public string StoryKey { get; set; }
        public string Summary { get; set; }
        public int StoryPoints { get; set; }
        public string ProjectKey { get; set; }
    }
}