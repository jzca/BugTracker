using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BugTracker.Models.Domain;

namespace BugTracker.Models.ViewModel.Ticket
{
    public class AttachmentDetailViewModel
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual ApplicationUser Creator { get; set; }
        public string CreatorId { get; set; }
        public bool OwnerAttachment { get; set; }
    }
}