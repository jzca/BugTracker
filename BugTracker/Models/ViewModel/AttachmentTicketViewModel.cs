using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class TicketAttachmentViewModel
    {
        public HttpPostedFileBase Media { get; set; }
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public int TicketId { get; set; }
        public string CreatorId { get; set; }

        //public virtual Ticket Ticket { get; }
        //public virtual ApplicationUser Creator { get; set; }
    }
}