using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModel
{
    public class CreateEditTicketViewModel
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public SelectList ProjectBelong { get; set; }
        public SelectList TicketType { get; set; }
        public SelectList TicketStatus { get; set; }
        public SelectList TicketPriority { get; set; }

        public string GetProjectBelong { get; set; }
        public string GetTicketType { get; set; }
        public string GetTicketStatus { get; set; }
        public string GetTicketPriority { get; set; }
    }
}