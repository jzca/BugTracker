using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel.Ticket
{
    public class DetailTicketViewModel
    {
        public DetailTicketViewModel()
        {

            TicketComments = new List<CommentDetailViewModel>();
            TicketAttachments = new List<AttachmentDetailViewModel>();
            TicketHistories = new List<HistoryDetailViewModel>();
        }


        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string AssignedDev { get; set; }
        public string ProjectName { get; set; }
        public string CreatorName { get; set; }
        public string TicketType { get; set; }
        public string TicketStatus { get; set; }
        public string TicketPriority { get; set; }
        public AttachmentTicketViewModel AttachmentVM { get; set; }
        public CommentTicketViewModel CommentVM { get; set; }

        public virtual List<CommentDetailViewModel> TicketComments { get; set; }
        public virtual List<AttachmentDetailViewModel> TicketAttachments { get; set; }
        public virtual List<HistoryDetailViewModel> TicketHistories { get; set; }

        public bool AreYouOwner { get; set; }

    }
}