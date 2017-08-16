using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimePilot.Entities
{
    public class Epic
    {
        public string EpicKey { get; set; }
        public string Summary { get; set; }
        public string ProjectKey { get; set; }
        public List<Story> storiesInEpic { get; set; }
    }
}