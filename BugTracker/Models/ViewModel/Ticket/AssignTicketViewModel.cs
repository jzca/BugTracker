using BugTracker.Models.Domain;
using BugTracker.Models.ViewModel.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel.Ticket
{
    public class AssignTicketViewModel
    {
        public int TicketId { get; set; }
        public string TicketTitle { get; set; }

        public AssignTicketViewModel()
        {
            Devs = new List<UserProjectViewModel>();
        }

        public virtual List<UserProjectViewModel> Devs { get; }
        public virtual UserProjectViewModel MyUser { get; set; }

    }
}