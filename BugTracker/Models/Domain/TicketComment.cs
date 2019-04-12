using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class TicketComment
    {
        public TicketComment()
        {
            Creator = new ApplicationUser();
        }

        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual ApplicationUser Creator { get; set; }
        public string CreatorId { get; set; }
    }
}