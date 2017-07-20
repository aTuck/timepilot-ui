using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using TimePilot.Web.Models;
using TimePilot.Entities;

namespace TimePilot.Web.ViewModels
{
    public class ResourceCapacityViewModel
    {
        public List<Sprint> sprints { get; set; }
        public List<Member> members { get; set; }
        
        public float totalDevCapacity { get; set; }
        public List<int> pageState { get; set; }
    }
}