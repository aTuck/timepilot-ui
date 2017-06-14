using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TimePilot.Web.Models
{
    public class Story
    {
        [Display(Name = "Description")]
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("summary")]
        public string Summary { get; set; }
        [JsonProperty("storyPoints")]
        public string StoryPoint { get; set; }
        public int IntStoryPoint { get; set; }
        public int storyId { get; set; }
        public Boolean isSelectedToDelete { get; set; }
    }
}