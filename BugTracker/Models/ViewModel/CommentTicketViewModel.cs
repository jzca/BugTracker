using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel
{
    public class CommentTicketViewModel
    {

        public int Id { get; set; }
        [Required]
        public string Comment { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatorId { get; set; }

    }
}