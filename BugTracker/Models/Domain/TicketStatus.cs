﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class TicketStatus
    {
        public TicketStatus()
        {
            Tickets = new List<Ticket>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Ticket> Tickets { get; }
    }
}