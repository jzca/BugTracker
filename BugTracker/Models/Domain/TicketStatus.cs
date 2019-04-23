using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    //[Table("TicketStatuses")]
    public class TicketStatus
    {
        public TicketStatus()
        {
            Tickets = new List<Ticket>();
        }

        //[Column("n_id")]
        public int Id { get; set; }

        [Required]
        [StringLength(25)]
        public string Name { get; set; }

        public virtual List<Ticket> Tickets { get; set; }
    }
}