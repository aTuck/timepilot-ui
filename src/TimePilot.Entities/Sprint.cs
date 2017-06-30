using System;

namespace TimePilot.Entities
{
    public class Sprint
    {
        public int SprintID { get; set; }
        public string Name { get; set; }
        public int SprintDays { get; set; }
        public int PercentWork { get; set; }
        public float StandupDuration { get; set; }
        public int Misc { get; set; }
        public int TimeOff { get; set; }
        public string ProjectKey { get; set; }
    }
}