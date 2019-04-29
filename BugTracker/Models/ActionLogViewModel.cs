using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class ActionLogViewModel
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public DateTime AccessTime { get; set; }
        //public int VisitsAction { get; set; }
    }
}