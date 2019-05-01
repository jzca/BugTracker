using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class Project
    {
        public Project()
        {
            Users = new List<ApplicationUser>();
            Tickets = new List<Ticket>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public virtual List<ApplicationUser> Users { get; }
        public virtual List<Ticket> Tickets { get; }
        public bool Archived { get; set; }
    }
}