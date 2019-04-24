using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel.Ticket
{
    public class EditCommentTicketViewModel
    {

        public int Id { get; set; }
        [Required]
        public string Comment { get; set; }
        public int TicketId { get; set; }

    }
}