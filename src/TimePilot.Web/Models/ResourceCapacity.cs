using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimePilot.Models
{
    public class ResourceCapacity
    {
        public List<Sprint> sprints { get; set; }
        public float avgPerSprint { get; set; }
        public float avgPerWeek { get; set; }
        public float totalDevCapacity { get; set; }
    }
}