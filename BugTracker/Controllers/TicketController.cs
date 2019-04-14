using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Helper;
using BugTracker.Models.ViewModel;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private ApplicationDbContext DbContext;
        private readonly UserRoleHelper UserRoleHelper;

        public TicketController()
        {
            DbContext = new ApplicationDbContext();
            UserRoleHelper = new UserRoleHelper(DbContext);
        }


        public ActionResult Index()
        {
            var allTickets = DbContext.Tickets.ToList();

            var model = allTickets.Select(b => new IndexTicketViewModel
            {
                Id = b.Id,
                Title = b.Title,
                AssignedDev = b.Assignee?.DisplayName ?? "Not assgined",
                CreatorName = b.Creator.DisplayName,
                ProjectName = b.Project.Name,
                TicketPriority = b.TicketPriority.Name,
                TicketStatus = b.TicketStatus.Name,
                TicketType = b.TicketType.Name,
                DateCreated = b.DateCreated,
                DateUpdated = b.DateUpdated,

            }).ToList();

            return View(model);
        }

        public ActionResult IndexIndividual()
        {
            var appUserId = User.Identity.GetUserId();
            var currentUser = DbContext.Users.Where(i => i.Id == appUserId).FirstOrDefault();

            var model = currentUser.CreatedTickets.Select(b => new IndexTicketViewModel
            {
                Id = b.Id,
                Title = b.Title,
                AssignedDev = b.Assignee?.DisplayName ?? "Not assgined",
                ProjectName = b.Project.Name,
                TicketPriority = b.TicketPriority.Name,
                TicketStatus = b.TicketStatus.Name,
                TicketType = b.TicketType.Name,
                DateCreated = b.DateCreated,
                DateUpdated = b.DateUpdated,

            }).ToList();

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Submitter")]
        public ActionResult Create()
        {
            var appUserId = User.Identity.GetUserId();
            //var subPjts = DbContext.Users.Where(p => p.Id == appUserId).Select(b=> new Project {}).ToList();
            var subPjts = DbContext.Projects.Where(p => p.Users.Any(m => m.Id == appUserId)).ToList();

            var model = new CreateEditTicketViewModel();
            model.ProjectBelong = new SelectList(subPjts, "Id", "Name");
            model.TicketType = new SelectList(DbContext.TicketTypes, "Id", "Name");
            model.TicketPriority = new SelectList(DbContext.TicketPriorities, "Id", "Name");
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Submitter")]
        public ActionResult Create(CreateEditTicketViewModel formData)
        {
            return SaveTicket(null, formData);
        }

        private ActionResult SaveTicket(int? id, CreateEditTicketViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var appUserId = User.Identity.GetUserId();

            Ticket ticketForSaving;

            if (!id.HasValue)
            {
                ticketForSaving = new Ticket();
                ticketForSaving.DateCreated = DateTime.Now;
                ticketForSaving.CreatorId = appUserId;
                ticketForSaving.ProjectId = Convert.ToInt32(formData.GetProjectBelong);
                ticketForSaving.TicketStatusId = 1;
                DbContext.Tickets.Add(ticketForSaving);
            }
            else
            {
                ticketForSaving = DbContext.Tickets.FirstOrDefault(
               p => p.Id == id);
                ticketForSaving.DateUpdated = DateTime.Now;

                if (User.IsInRole("Admin") || User.IsInRole("Project Manager"))
                {
                    ticketForSaving.TicketStatusId = Convert.ToInt32(formData.GetTicketStatus);
                }

                if (ticketForSaving == null)
                {
                    return RedirectToAction(nameof(TicketController.Index));
                }
            }

            ticketForSaving.Title = formData.Title;
            ticketForSaving.Description = formData.Description;
            ticketForSaving.TicketPriorityId = Convert.ToInt32(formData.GetTicketPriority);
            ticketForSaving.TicketTypeId = Convert.ToInt32(formData.GetTicketType);

            DbContext.SaveChanges();



            return RedirectToAction(nameof(TicketController.Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager, Submitter")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketController.Index));
            }

            var ticket = DbContext.Tickets.FirstOrDefault(
                p => p.Id == id.Value);

            if (ticket == null)
            {
                return RedirectToAction(nameof(TicketController.Index));
            }

            var model = new CreateEditTicketViewModel();
            model.Title = ticket.Title;
            model.Description = ticket.Description;
            model.GetTicketPriority = Convert.ToString(ticket.TicketPriorityId);
            model.GetTicketType = Convert.ToString(ticket.TicketTypeId);
            model.TicketType = new SelectList(DbContext.TicketTypes, "Id", "Name");
            model.TicketPriority = new SelectList(DbContext.TicketPriorities, "Id", "Name");
            if (User.IsInRole("Admin") || User.IsInRole("Project Manager"))
            {
                model.GetTicketStatus = Convert.ToString(ticket.TicketStatusId);
                model.TicketStatus = new SelectList(DbContext.TicketStatuses, "Id", "Name");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager, Submitter")]
        public ActionResult Edit(int id, CreateEditTicketViewModel formData)
        {
            return SaveTicket(id, formData);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AssignTicketManagement(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketController.Index));
            }

            //var roleDevId = DbContext.Roles.Where(p => p.Name == "Developer").Select(b => b.Id).FirstOrDefault();

            var allDevs = DbContext.Users.Where(p => p.Roles.Any(b => b.RoleId ==
            DbContext.Roles.Where(m => m.Name == "Developer").Select(n => n.Id).FirstOrDefault()))
                .Select(n => new UserProjectViewModel
                {
                    Id = n.Id,
                    UserName = n.UserName
                }).ToList();

            var ticket = DbContext.Tickets.FirstOrDefault(
                p => p.Id == id.Value);

            var repeated = new UserProjectViewModel();

            repeated = allDevs.Where(m => m.Id == ticket.AssigneeId).FirstOrDefault();
            allDevs.Remove(repeated);


            if (ticket == null)
            {
                return RedirectToAction(nameof(TicketController.Index));
            }

            var model = new AssignTicketViewModel();

            model.TicketId = ticket.Id;
            model.TicketTitle = ticket.Title;
            //model.MyUser.Id = ticket.AssigneeId ?? "No Assignee";
            if (ticket.AssigneeId != null || ticket.Assignee != null)
            {
                model.MyUser = new UserProjectViewModel();
                model.MyUser.Id = ticket.AssigneeId;
                model.MyUser.UserName = ticket.Assignee.UserName;
            }
            else
            {
                model.MyUser = new UserProjectViewModel();
                model.MyUser.Id = "NoAssigneeId";
                //model.MyUser.UserName = "No Assignee";
            }


            model.Devs.AddRange(allDevs);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Assign(int tkId, string userId)
        {
            //if (!ModelState.IsValid)
            //{
            //    return RedirectToAction(nameof(ProjectController.AssignManagement), new { id = pJid });
            //}

            var ticket = DbContext.Tickets.FirstOrDefault(p => p.Id == tkId);
            var user = DbContext.Users.FirstOrDefault(p => p.Id == userId);

            //var leftUsers = DbContext.Users
            //    .Where(p => p.Id != userId)
            //    .Select(n => new UserProjectViewModel
            //    {
            //        Id = n.Id,
            //        UserName = n.UserName
            //    }).ToList();


            bool freshTicket = true;
            if (user.AssignedTickets.Any())
            {
                freshTicket = user.AssignedTickets.Any(p => p.Id != ticket.Id);
            }

            if (freshTicket)
            {
                user.AssignedTickets.Add(ticket);
                DbContext.SaveChanges();
            }
            //else
            //{
            //    ModelState.AddModelError(nameof(ProjectController.AssignManagement),
            //    "Already Assigned");
            //}


            return RedirectToAction(nameof(TicketController.AssignTicketManagement), new { id = tkId });
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult UnAssign(int tkId, string userId)
        {
            var ticket = DbContext.Tickets.FirstOrDefault(p => p.Id == tkId);
            var user = DbContext.Users.FirstOrDefault(p => p.Id == userId);

            bool includedTicket = user.AssignedTickets.Any(p => p.Id == ticket.Id);
            if (includedTicket)
            {
                user.AssignedTickets.Remove(ticket);
                DbContext.SaveChanges();
            }

            return RedirectToAction(nameof(TicketController.AssignTicketManagement), new { id = tkId });
        }

    }
}
