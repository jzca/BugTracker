using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModel.Ticket
{
    public class CreateEditTicketViewModel
    {

        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public SelectList ProjectBelong { get; set; }
        public SelectList TicketType { get; set; }
        public SelectList TicketStatus { get; set; }
        public SelectList TicketPriority { get; set; }
        [Required]
        public string GetProjectBelong { get; set; }
        [Required]
        public string GetTicketType { get; set; }
        [Required]
        public string GetTicketPriority { get; set; }
        public string GetTicketStatus { get; set; }
    }
}