using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel.Ticket
{
    public class CommentDetailViewModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime DateCreated { get; set; }
        //public virtual Ticket Ticket { get; }
        //public int TicketId { get; set; }
        public virtual ApplicationUser Creator { get; set; }
        public string CreatorId { get; set; }
        public bool OwnerComment { get; set; }
    }
}