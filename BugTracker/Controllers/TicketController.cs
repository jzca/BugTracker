using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Helper;
using BugTracker.Models.ViewModel;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
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

        public ActionResult IndexSubInProjects()
        {
            var appUserId = User.Identity.GetUserId();
            var currentUser = DbContext.Users.Where(i => i.Id == appUserId).FirstOrDefault();


            //var allTickets = DbContext.Users.P
            //    Tickets.ToList();

            //var model = currentUser.Projects.Select( p=> p.Tickets).Select(b => new IndexTicketViewModel
            //{
            //    Id = b.Id,
            //    Title = b.Title,
            //    AssignedDev = b.Assignee?.DisplayName ?? "Not assgined",
            //    CreatorName = b.Creator.DisplayName,
            //    ProjectName = b.Project.Name,
            //    TicketPriority = b.TicketPriority.Name,
            //    TicketStatus = b.TicketStatus.Name,
            //    TicketType = b.TicketType.Name,
            //    DateCreated = b.DateCreated,
            //    DateUpdated = b.DateUpdated,

            //}).ToList();

            return View();
        }



        public ActionResult IndexSubCreated()
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

        public ActionResult IndexSubAll()
        {
            var appUserId = User.Identity.GetUserId();

            var currentUserDisplayName = DbContext.Users
                                            .Where(p => p.Id == appUserId)
                                            .Select(b => b.DisplayName)
                                            .FirstOrDefault();

            var allTickets = DbContext.Tickets
                .Where(n => n.Project.Users.Any(m => m.Id == appUserId)
                || n.CreatorId == appUserId)
                .ToList();

            var model = allTickets.Select(b => new IndexTicketViewModel
            {
                Id = b.Id,
                Title = b.Title,
                AssignedDev = b.Assignee?.DisplayName ?? "Not assgined",
                ProjectName = b.Project.Name,
                CreatorName = b.Creator.DisplayName,
                TicketPriority = b.TicketPriority.Name,
                TicketStatus = b.TicketStatus.Name,
                TicketType = b.TicketType.Name,
                DateCreated = b.DateCreated,
                DateUpdated = b.DateUpdated,

            }).ToList();


            foreach (var m in model)
            {
                if (m.CreatorName != currentUserDisplayName)
                {
                    m.IsCreator = false;
                }
                else
                {
                    m.IsCreator = true;
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Developer")]
        public ActionResult IndexDevAssigned()
        {
            var appUserId = User.Identity.GetUserId();

            //var currentUser = DbContext.Users.Where(i => i.Id == appUserId).FirstOrDefault();

            //var tickets =DbContext.Projects.Where(p => p.Users.Contains(currentUser))
            //    .Select(n => n.Tickets).ToList();

            var ticketAssigned = DbContext.Users.Where(i => i.Id == appUserId)
                .FirstOrDefault().
                AssignedTickets.ToList();

            var model = ticketAssigned.Select(b => new IndexDevAssignedTicketViewModel
            {
                Id = b.Id,
                Title = b.Title,
                ProjectName = b.Project.Name,
                TicketPriority = b.TicketPriority.Name,
                TicketStatus = b.TicketStatus.Name,
                TicketType = b.TicketType.Name,
                DateCreated = b.DateCreated,
                DateUpdated = b.DateUpdated,

            }).ToList();

            return View(model);
        }

        [Authorize(Roles = "Developer")]
        public ActionResult IndexDevAll()
        {
            var appUserId = User.Identity.GetUserId();

            var currentUserDisplayName = DbContext.Users
                                .Where(p => p.Id == appUserId)
                                .Select(b => b.DisplayName)
                                .FirstOrDefault();

            var allTickets = DbContext.Tickets
                .Where(n => n.Project.Users.Any(m => m.Id == appUserId)
                || n.AssigneeId == appUserId)
                .ToList();

            //var ticketsBelongMyProject = DbContext.Users.Where(i => i.Id == appUserId).FirstOrDefault()
            //    .Projects.SelectMany(p => p.Tickets).ToList();

            var model = allTickets.Select(b => new IndexDevAllTicketViewModel
            {
                Id = b.Id,
                Title = b.Title,
                ProjectName = b.Project.Name,
                CreatorName = b.Creator.DisplayName,
                AssigneeName = b.Assignee?.DisplayName ?? "Not assgined",
                TicketPriority = b.TicketPriority.Name,
                TicketStatus = b.TicketStatus.Name,
                TicketType = b.TicketType.Name,
                DateCreated = b.DateCreated,
                DateUpdated = b.DateUpdated,

            }).ToList();

            foreach (var m in model)
            {
                if (m.AssigneeName != currentUserDisplayName)
                {
                    m.IsAssignee = false;
                }
                else
                {
                    m.IsAssignee = true;
                }
            }


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

                if (ticketForSaving == null)
                {
                    return RedirectToAction(nameof(TicketController.Index));
                }

                #region Ownership Check

                bool isAdmin = User.IsInRole("Admin");
                bool isProjm = User.IsInRole("Project Manager");
                bool isSubmitter = User.IsInRole("Submitter");
                bool isDeveloper = User.IsInRole("Developer");

                if (isSubmitter && !(isAdmin || isProjm))
                {
                    bool isNotCreator = ticketForSaving.CreatorId != appUserId;
                    if (isNotCreator)
                    {
                        return RedirectToAction(nameof(TicketController.IndexSubAll));
                    }

                }

                if (isDeveloper && !(isAdmin || isProjm))
                {
                    bool isNotAssignee = ticketForSaving.AssigneeId != appUserId;

                    if (isNotAssignee && !(isAdmin || isProjm))
                    {
                        return RedirectToAction(nameof(TicketController.IndexDevAll));
                    }

                }
                #endregion

                ticketForSaving.DateUpdated = DateTime.Now;

                if (User.IsInRole("Admin") || User.IsInRole("Project Manager"))
                {
                    ticketForSaving.TicketStatusId = Convert.ToInt32(formData.GetTicketStatus);
                }
            }

            ticketForSaving.Title = formData.Title;
            ticketForSaving.Description = formData.Description;
            ticketForSaving.TicketPriorityId = Convert.ToInt32(formData.GetTicketPriority);
            ticketForSaving.TicketTypeId = Convert.ToInt32(formData.GetTicketType);

            DbContext.SaveChanges();



            return RedirectToAction(nameof(TicketController.Index));
        }

        //private ActionResult OwnershipCheckForEdit(string userId, Ticket ticket)
        //{
        //    #region Ownership Check

        //    bool isAdmin = User.IsInRole("Admin");
        //    bool isProjm = User.IsInRole("Project Manager");
        //    bool isSubmitter = User.IsInRole("Submitter");
        //    bool isDeveloper = User.IsInRole("Developer");

        //    if (isSubmitter && !(isAdmin || isProjm))
        //    {
        //        bool isNotCreator = ticket.CreatorId != userId;
        //        if (isNotCreator)
        //        {
        //            return RedirectToAction(nameof(TicketController.IndexSubAll));
        //        }

        //    }

        //    if (isDeveloper && !(isAdmin || isProjm))
        //    {
        //        bool isNotAssignee = ticket.AssigneeId != userId;

        //        if (isNotAssignee)
        //        {
        //            return RedirectToAction(nameof(TicketController.IndexDevAll));
        //        }

        //    }
        //    #endregion
        //    // End of Check

        //    return RedirectToAction(nameof(TicketController.Index));
        //}



        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager, Submitter, Developer")]
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

            var appUserId = User.Identity.GetUserId();

            #region Ownership Check

            bool isAdmin = User.IsInRole("Admin");
            bool isProjm = User.IsInRole("Project Manager");
            bool isSubmitter = User.IsInRole("Submitter");
            bool isDeveloper = User.IsInRole("Developer");

            if (isSubmitter && !(isAdmin || isProjm))
            {
                bool isNotCreator = ticket.CreatorId != appUserId;
                if (isNotCreator)
                {
                    return RedirectToAction(nameof(TicketController.IndexSubAll));
                }

            }

            if (isDeveloper && !(isAdmin || isProjm))
            {
                bool isNotAssignee = ticket.AssigneeId != appUserId;

                if (isNotAssignee)
                {
                    return RedirectToAction(nameof(TicketController.IndexDevAll));
                }

            }
            #endregion


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
        [Authorize(Roles = "Admin, Project Manager, Submitter, Developer")]
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
            model.MyUser = new UserProjectViewModel();

            model.TicketId = ticket.Id;
            model.TicketTitle = ticket.Title;
            //model.MyUser.Id = ticket.AssigneeId ?? "No Assignee";
            if (ticket.AssigneeId != null || ticket.Assignee != null)
            {
                model.MyUser.Id = ticket.AssigneeId;
                model.MyUser.UserName = ticket.Assignee.UserName;
            }
            else
            {
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

        public ActionResult Detail(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction(nameof(TicketController.Index));
            var appUserId = User.Identity.GetUserId();

            var ticket = DbContext.Tickets.Where(p => p.Id == id.Value).FirstOrDefault();

            var model = new DetailTicketViewModel();

            model.Id = ticket.Id;
            model.Title = ticket.Title;
            model.Description = ticket.Description;
            model.TicketPriority = ticket.TicketPriority.Name;
            model.TicketStatus = ticket.TicketStatus.Name;
            model.TicketType = ticket.TicketType.Name;
            model.AssignedDev = ticket.Assignee?.DisplayName ?? "None";
            model.ProjectName = ticket.Project.Name;
            model.CreatorName = ticket.Creator.DisplayName;
            model.DateCreated = ticket.DateCreated;
            model.DateUpdated = ticket.DateUpdated;
            model.TicketAttachments = ticket.TicketAttachments;
            model.TicketComments = ticket.TicketComments;

            return View(model);

        }

        //[HttpGet]
        //public ActionResult Attachment(int? id)
        //{
        //    if (!id.HasValue || id.HasValue)
        //    {
        //        return RedirectToAction(nameof(TicketController.Index));
        //    }

        //    return View();
        //}

        [HttpPost]
        public ActionResult Attachment(int id, AttachmentTicketViewModel formData)
        {
            //var ticket = DbContext.Tickets.Where(p => p.Id == id).FirstOrDefault();
            var appUserId = User.Identity.GetUserId();

            //// Handling file upload
            if (formData.Media != null)
            {
                //// Validating file upload
                var fileExtensionForSaving = Path.GetExtension(formData.Media.FileName).ToLower();

                if (!AttachmentHandler.AllowedFileExtensions.Contains(fileExtensionForSaving))
                {
                    ModelState.AddModelError("", "File extension is not allowed.");
                    ViewBag.ErroMsg = "File extension is not allowed.";
                    return View();
                }


                if (!Directory.Exists(AttachmentHandler.MappedUploadFolder))
                {
                    Directory.CreateDirectory(AttachmentHandler.MappedUploadFolder);
                }

                var fileName = formData.Media.FileName;
                var fullPathWithName = AttachmentHandler.MappedUploadFolder + fileName;

                formData.Media.SaveAs(fullPathWithName);
                var uploadFile = new TicketAttachment()
                {
                    TicketId = id,
                    CreatorId = appUserId,
                    FilePath = fullPathWithName,
                    FileUrl = AttachmentHandler.AttachmentSaveFolder + fileName,
                    DateCreated = DateTime.Now,
                    Description = formData.Description

                };

                DbContext.TicketAttachments.Add(uploadFile);
            }


            DbContext.SaveChanges();


            return RedirectToAction(nameof(TicketController.Detail), new { id });
        }


        [HttpPost]
        public ActionResult Comment(int id, CommentTicketViewModel formData)
        {

            var appUserId = User.Identity.GetUserId();
            var freshComment = new TicketComment()
            {
                TicketId = id,
                Comment = formData.Comment,
                CreatorId = appUserId,
                DateCreated = DateTime.Now
            };

            DbContext.TicketComments.Add(freshComment);

            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketController.Detail), new { id });
        }








    }
}
