using BugTracker.Models;
using BugTracker.Models.Domain;
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

        public TicketController()
        {
            DbContext = new ApplicationDbContext();
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
            model.TicketStatus = new SelectList(DbContext.TicketStatuses, "Id", "Name");
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
                DbContext.Tickets.Add(ticketForSaving);
            }
            else
            {
                ticketForSaving = DbContext.Tickets.FirstOrDefault(
               p => p.Id == id);

                ticketForSaving.DateUpdated = DateTime.Now;

                if (ticketForSaving == null)
                {
                    return RedirectToAction(nameof(TicketController.Index));
                }
            }

            ticketForSaving.Title = formData.Title;
            ticketForSaving.Description = formData.Description;
            ticketForSaving.TicketPriorityId = Convert.ToInt32(formData.GetTicketPriority);
            ticketForSaving.TicketStatusId = Convert.ToInt32(formData.GetTicketStatus);
            ticketForSaving.TicketTypeId = Convert.ToInt32(formData.GetTicketType);

            DbContext.SaveChanges();



            return RedirectToAction(nameof(TicketController.Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
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
            model.GetTicketStatus = Convert.ToString(ticket.TicketStatusId);
            model.GetTicketType = Convert.ToString(ticket.TicketTypeId);
            model.TicketType = new SelectList(DbContext.TicketTypes, "Id", "Name");
            model.TicketPriority = new SelectList(DbContext.TicketPriorities, "Id", "Name");
            model.TicketStatus = new SelectList(DbContext.TicketStatuses, "Id", "Name");

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Edit(int id, CreateEditTicketViewModel formData)
        {
            return SaveTicket(id, formData);
        }

    }
}
