using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimePilot.Entities
{
    public class Member
    {
        public int MemberID { get; set; }
        public string Name { get; set; }
        public int SprintDays { get; set; }
        public int PercentWork { get; set; }
        public float StandupDuration { get; set; }
        public int Misc { get; set; }
        public int TimeOff { get; set; }
        public int SprintID{ get; set; }

        public int PageID { get; set; }
    }
}