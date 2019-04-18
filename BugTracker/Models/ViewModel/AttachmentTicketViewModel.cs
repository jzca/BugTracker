using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel
{
    public class AttachmentTicketViewModel
    {
        [Required]
        public HttpPostedFileBase Media { get; set; }
        public int Id { get; set; }
        public string FilePath { get; set; }
        [Required]
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public int TicketId { get; set; }
        public string CreatorId { get; set; }
    }
}