using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel.Ticket
{
    public class HistoryDetailViewModel
    {
        public int Id { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string ModifierName { get; set; }
        public DateTime TimeStamp { get; set; }
        //public int TicketId { get; set; }
        //public string ModifierId { get; set; }
    }
}