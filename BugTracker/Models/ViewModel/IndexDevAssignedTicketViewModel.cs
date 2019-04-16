using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel
{
    public class IndexDevAssignedTicketViewModel
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string ProjectName { get; set; }
        public string CreatorName { get; set; }
        public string TicketType { get; set; }
        public string TicketStatus { get; set; }
        public string TicketPriority { get; set; }

    }
}