using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace BugTracker.Models.Helper
{
    public class AppHepler
    {
        private readonly ApplicationDbContext DbContext;

        public AppHepler(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }


        public enum WhichRole
        {
            Admin,
            ProjectManager,
            Developer,
            Submitter
        }


        public Project GetProjectById(int? id)
        {
            return DbContext.Projects.FirstOrDefault(
               p => p.Id == id.Value);
        }

        public List<Project> GetAllProjects()
        {
            return DbContext.Projects.ToList();
        }

        public List<Project> GetProjects4CurrentUser(ApplicationUser user)
        {
            return user.Projects;
        }
        public List<Project> GetProjects4CurrentUserById(string userId)
        {
            return DbContext.Projects
                .Where(p => p.Users.Any(m => m.Id == userId)).ToList();
        }

        public ApplicationUser GetUserById(string userId)
        {
            return DbContext.Users.FirstOrDefault(p => p.Id == userId);
        }

        public List<ApplicationUser> GetAllUsers()
        {
            return DbContext.Users.ToList();
        }

        public string GetDisplayName4CurrentUserById(string userId)
        {
            return DbContext.Users
                            .Where(p => p.Id == userId)
                            .Select(b => b.DisplayName)
                            .FirstOrDefault();
        }


        public Ticket GetTicketById(int? tkId)
        {
            return DbContext.Tickets.FirstOrDefault(p => p.Id == tkId.Value);
        }


        public List<Ticket> GetAllTickets()
        {
            return DbContext.Tickets.ToList();
        }

        public List<Ticket> GetAllTicketsForDevOrSub(WhichRole who, string userId)
        {

            List<Ticket> ticketsGot = new List<Ticket>();

            if (who == WhichRole.Developer)
            {
                ticketsGot = DbContext.Tickets
                .Where(n => n.Project.Users.Any(m => m.Id == userId)
                || n.AssigneeId == userId)
                .ToList();
            }
            else if (who == WhichRole.Submitter)
            {
                ticketsGot = DbContext.Tickets
                .Where(n => n.Project.Users.Any(m => m.Id == userId)
                || n.CreatorId == userId)
                .ToList();
            }

            return ticketsGot;
        }

        public int GetDefaultTicketStatus()
        {
            return DbContext.TicketStatuses
                    .Where(p => p.Name == TicketEnum.Status.Open.ToString())
                    .Select(b => b.Id).FirstOrDefault();
        }


        public TicketComment GetTicketCommentById(int? cmId)
        {
            return DbContext.TicketComments.FirstOrDefault(
                p => p.Id == cmId.Value);
        }

        public TicketAttachment GetTicketAttachmentById(int? atmId)
        {
            return DbContext.TicketAttachments.Where(p => p.Id == atmId.Value).FirstOrDefault();
        }

        public TicketPriority GetTkPriorityById(int id)
        {
            return DbContext.TicketPriorities.FirstOrDefault(p => p.Id == id);
        }

        public TicketStatus GetTkStatusById(int id)
        {
            return DbContext.TicketStatuses.FirstOrDefault(p => p.Id == id);
        }
        public TicketType GetTkTypeyById(int id)
        {
            return DbContext.TicketTypes.FirstOrDefault(p => p.Id == id);
        }

    }
}