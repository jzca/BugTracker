using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public virtual Ticket Ticket { get; }
        public int TicketId { get; set; }
        public virtual ApplicationUser Modifier { get; set; }
        public string ModifierId { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}