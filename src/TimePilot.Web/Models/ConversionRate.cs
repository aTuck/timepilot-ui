using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimePilot.Web.Models
{
    public class ConversionRate
    {
        public int id { get; set; }
        public int storyPoints { get; set; }
        public int conversionRate { get; set; }
    }
}