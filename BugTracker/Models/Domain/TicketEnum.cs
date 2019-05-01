using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class TicketEnum
    {
        public enum Type { Bug, Feature, Database, Support }
        public enum Priority { Low, Medium, High }
        public enum Status { Open, Resolved, Rejected }

        public enum HistoryProperty
        {
            ProjectBelong,
            TicketTitle,
            TicketType,
            TicketPriority,
            TicketStatus,
            TicketDescription,
            Assignee,
        }
        public enum PrevValue
        {
            TicketStatusId,
            Title,
            Description,
            ProjectId,
            TicketPriorityId,
            TicketTypeId


        }
        public enum NextValue
        {
            GetTicketStatus,
            Title,
            Description,
            GetProjectBelong,
            GetTicketPriority,
            GetTicketType

        }

    }
}