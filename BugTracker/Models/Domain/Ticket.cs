using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class Ticket
    {
        public Ticket()
        {
            TicketComments = new List<TicketComment>();
            TicketAttachments = new List<TicketAttachment>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual ApplicationUser Creator { get; set;}
        public string  CreatorId { get; set; }

        public virtual ApplicationUser Assignee { get; set; }
        public string AssigneeId { get; set; }

        public virtual Project  Project { get; set; }
        public int ProjectId { get; set; }

        public virtual List<TicketComment> TicketComments { get; set; }
        public virtual List<TicketAttachment> TicketAttachments { get; set; }
        public virtual TicketType TicketType { get; set; }
        public int TicketTypeId { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public int TicketStatusId { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public int TicketPriorityId { get; set; }


    }
}