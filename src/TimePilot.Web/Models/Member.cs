using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimePilot.Web.Models
{
    public class Member
    {
        public String name { get; set; }
        public String role { get; set; }
        public float sprintDays { get; set; }
        public float percentWork { get; set; }
        public float totalHours { get; set; }
        public float standupDuration { get; set; }
        public float standUps { get; set; }
        public float misc { get; set; }
        public float timeOff { get; set; }
        public float nonDevHours { get; set; }
        public float totalAvailable { get; set; }
        public int memberCount { get; set; }
        public List<MemberSprintData> memberSprintData { get; set; }
    }
}