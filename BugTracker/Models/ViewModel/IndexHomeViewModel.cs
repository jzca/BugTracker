using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel
{
    public class IndexHomeViewModel
    {
        public int NumOpenTk { get; set; }
        public int NumResolvedTk { get; set; }
        public int NumRejectedTk { get; set; }

        public int NumOwnedProjects { get; set; }
        public int NumOwnedTickets { get; set; }

        public bool YesAdmin { get; set; }
        public bool YesDev { get; set; }
        public bool YesPm { get; set; }
        public bool YesSub { get; set; }

    }
}