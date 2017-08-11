using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimePilot.Entities
{
    public class Conversion
    {
        public int ConversionRateID { get; set; }
        public int StoryPoints { get; set; }
        public float ConversionRate { get; set; }
        public string  ProjectKey { get; set; }
    }
}