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

        [Authorize(Roles = "Admin, Project Manager")]
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

        [Authorize(Roles = "Submitter")]
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

        [Authorize(Roles = "Submitter")]
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
            var subPjts = DbContext.Projects.Where(p => p.Users.Any(m => m.Id == appUserId)).ToList();

            var model = new CreateEditTicketViewModel();
            model.ProjectBelong = new SelectList(subPjts, "Id", "Name");
            model.TicketType = new SelectList(DbContext.TicketTypes, "Id", "Name");
            model.TicketPriority = new SelectList(DbContext.TicketPriorities, "Id", "Name");
            model.GetTicketStatus = Convert.ToString(DbContext.TicketStatuses
                    .Where(p => p.Name == TicketEnum.Status.Open.ToString())
                    .Select(b => b.Id).FirstOrDefault());
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
                return View(MStateNotValid(id));
            }

            var appUserId = User.Identity.GetUserId();

            Ticket ticketForSaving;

            if (!id.HasValue)
            {
                ticketForSaving = new Ticket();
                ticketForSaving.DateCreated = DateTime.Now;
                ticketForSaving.CreatorId = appUserId;
                ticketForSaving.TicketStatusId = DbContext.TicketStatuses
                    .Where(p => p.Name == TicketEnum.Status.Open.ToString())
                    .Select(b => b.Id).FirstOrDefault();
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

                // Check OwnerShip
                var result = OwnershipCheckEdit(appUserId, ticketForSaving);

                if (result == nameof(TicketController.IndexSubAll))
                {
                    return RedirectToAction(nameof(TicketController.IndexSubAll));
                }
                else if (result == nameof(TicketController.IndexDevAll))
                {
                    return RedirectToAction(nameof(TicketController.IndexDevAll));
                }


                ticketForSaving.DateUpdated = DateTime.Now;

                if (IsAdmin() || IsProjectManager())
                {
                    if (formData.GetTicketStatus == null)
                    {
                        ModelState.AddModelError("", "Ticket Status is Required!");
                        return View(MStateNotValid(id));
                    }

                    ticketForSaving.TicketStatusId = Convert.ToInt32(formData.GetTicketStatus);
                }
            }

            ticketForSaving.Title = formData.Title;
            ticketForSaving.Description = formData.Description;
            ticketForSaving.ProjectId = Convert.ToInt32(formData.GetProjectBelong);
            ticketForSaving.TicketPriorityId = Convert.ToInt32(formData.GetTicketPriority);
            ticketForSaving.TicketTypeId = Convert.ToInt32(formData.GetTicketType);

            DbContext.SaveChanges();


            if (IsSubmitter())
            {
                return RedirectToAction(nameof(TicketController.IndexSubAll));
            }

            if (IsDeveloper())
            {
                return RedirectToAction(nameof(TicketController.IndexDevAll));
            }


            return RedirectToAction(nameof(TicketController.Index));
        }


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

            // Check OwnerShip
            var result = OwnershipCheckEdit(appUserId, ticket);

            if (result == nameof(TicketController.IndexSubAll))
            {
                return RedirectToAction(nameof(TicketController.IndexSubAll));
            }
            else if (result == nameof(TicketController.IndexDevAll))
            {
                return RedirectToAction(nameof(TicketController.IndexDevAll));
            }


            var ownedPjts = DbContext.Projects.Where(p => p.Users.Any(m => m.Id == appUserId)).ToList();

            var model = new CreateEditTicketViewModel();
            model.Title = ticket.Title;
            model.Description = ticket.Description;
            model.TicketType = new SelectList(DbContext.TicketTypes, "Id", "Name");
            model.TicketPriority = new SelectList(DbContext.TicketPriorities, "Id", "Name");
            model.GetTicketPriority = Convert.ToString(ticket.TicketPriorityId);
            model.GetTicketType = Convert.ToString(ticket.TicketTypeId);

            if (IsAdmin() || IsProjectManager())
            {
                var allPjts = DbContext.Projects.ToList();
                model.ProjectBelong = new SelectList(allPjts, "Id", "Name");
                model.TicketStatus = new SelectList(DbContext.TicketStatuses, "Id", "Name");
                model.GetTicketStatus = Convert.ToString(ticket.TicketStatusId);
            }
            else
            {
                model.ProjectBelong = new SelectList(ownedPjts, "Id", "Name");
            }

            model.GetProjectBelong = Convert.ToString(ticket.ProjectId);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager, Submitter, Developer")]
        public ActionResult Edit(int id, CreateEditTicketViewModel formData)
        {
            return SaveTicket(id, formData);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AssignTicketManagement(int? id)
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

            var allDevs = DbContext.Users.Where(p => p.Roles.Any(b => b.RoleId ==
            DbContext.Roles.Where(m => m.Name == "Developer").Select(n => n.Id).FirstOrDefault()))
                .Select(n => new UserProjectViewModel
                {
                    Id = n.Id,
                    UserName = n.UserName
                }).ToList();

            var repeated = new UserProjectViewModel();

            repeated = allDevs.Where(m => m.Id == ticket.AssigneeId).FirstOrDefault();
            allDevs.Remove(repeated);

            var model = new AssignTicketViewModel();
            model.MyUser = new UserProjectViewModel();

            model.TicketId = ticket.Id;
            model.TicketTitle = ticket.Title;
            if (ticket.AssigneeId != null || ticket.Assignee != null)
            {
                model.MyUser.Id = ticket.AssigneeId;
                model.MyUser.UserName = ticket.Assignee.UserName;
            }
            else
            {
                model.MyUser.Id = "NoAssigneeId";
            }


            model.Devs.AddRange(allDevs);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Assign(int tkId, string userId)
        {

            var ticket = DbContext.Tickets.FirstOrDefault(p => p.Id == tkId);
            var user = DbContext.Users.FirstOrDefault(p => p.Id == userId);

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

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager, Submitter, Developer")]
        public ActionResult Detail(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction(nameof(TicketController.Index));
            var appUserId = User.Identity.GetUserId();

            var ticket = DbContext.Tickets.Where(p => p.Id == id.Value).FirstOrDefault();

            if (ticket == null)
            {
                return RedirectToAction(nameof(TicketController.Index));
            }

            // Check OwnerShip
            var result = OwnershipCheckDetail(appUserId, ticket);

            if (result == nameof(TicketController.IndexSubAll))
            {
                return RedirectToAction(nameof(TicketController.IndexSubAll));
            }
            else if (result == nameof(TicketController.IndexDevAll))
            {
                return RedirectToAction(nameof(TicketController.IndexDevAll));
            }

            bool isNotCreator = ticket.CreatorId != appUserId;
            bool isNotAssignee = ticket.AssigneeId != appUserId;

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
            model.AreYouOwner = true;

            if (!IsAdmin() && !IsProjectManager())
            {
                if (isNotCreator && isNotAssignee)
                {
                    model.AreYouOwner = false;
                }
            }

            return View(model);

        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager, Submitter, Developer")]
        public ActionResult Attachment(int id, AttachmentTicketViewModel formData)
        {
            var ticket = DbContext.Tickets.Where(p => p.Id == id).FirstOrDefault();
            var appUserId = User.Identity.GetUserId();

            if (!ModelState.IsValid)
            {
                return View("Detail", MStateNotValidDetail(ticket, appUserId));
            }

            // Check OwnerShip
            var result = OwnershipCheckEdit(appUserId, ticket);

            if (result == nameof(TicketController.IndexSubAll))
            {
                return RedirectToAction(nameof(TicketController.IndexSubAll));
            }
            else if (result == nameof(TicketController.IndexDevAll))
            {
                return RedirectToAction(nameof(TicketController.IndexDevAll));
            }

            //// Handling file upload
            if (formData.Media != null)
            {
                //// Validating file upload
                var fileExtensionForSaving = Path.GetExtension(formData.Media.FileName).ToLower();

                if (!AttachmentHandler.AllowedFileExtensions.Contains(fileExtensionForSaving))
                {
                    ModelState.AddModelError("", "File extension is not allowed.");
                    ViewBag.ErroMsg = "File extension is not allowed.";
                    return View("ErroMsg");
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
                DbContext.SaveChanges();

            }

            return RedirectToAction(nameof(TicketController.Detail), new { id });
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager, Submitter, Developer")]
        public ActionResult Comment(int id, CommentTicketViewModel formData)
        {
            var ticket = DbContext.Tickets.Where(p => p.Id == id).FirstOrDefault();
            var appUserId = User.Identity.GetUserId();

            if (!ModelState.IsValid)
            {
                return View("Detail", MStateNotValidDetail(ticket, appUserId));
            }

            // Check OwnerShip
            var result = OwnershipCheckEdit(appUserId, ticket);

            if (result == nameof(TicketController.IndexSubAll))
            {
                return RedirectToAction(nameof(TicketController.IndexSubAll));
            }
            else if (result == nameof(TicketController.IndexDevAll))
            {
                return RedirectToAction(nameof(TicketController.IndexDevAll));
            }

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

        private bool IsAdmin() { return User.IsInRole("Admin"); }
        private bool IsProjectManager() { return User.IsInRole("Project Manager"); }
        private bool IsSubmitter() { return User.IsInRole("Submitter"); }
        private bool IsDeveloper() { return User.IsInRole("Developer"); }

        private string OwnershipCheckEdit(string userId, Ticket ticket)
        {
            bool isAdmin = IsAdmin();
            bool isProjm = IsProjectManager();

            if (!isAdmin || !isProjm)
            {
                bool isSubmitter = IsSubmitter();
                bool isDeveloper = IsDeveloper();

                bool isNotCreator = ticket.CreatorId != userId;
                bool isNotAssignee = ticket.AssigneeId != userId;

                if (isNotCreator && isNotAssignee)
                {
                    if (isSubmitter && isNotCreator)
                    {
                        return nameof(TicketController.IndexSubAll);
                    }

                    if (isDeveloper && isNotAssignee)
                    {
                        return nameof(TicketController.IndexDevAll);
                    }
                }
            }
            return null;
        }

        private string OwnershipCheckDetail(string userId, Ticket ticket)
        {

            bool isAdmin = IsAdmin();
            bool isProjm = IsProjectManager();

            if (!isAdmin || !isProjm)
            {
                bool isSubmitter = IsSubmitter();
                bool isDeveloper = IsDeveloper();

                bool isNotCreator = ticket.CreatorId != userId;
                bool isNotAssignee = ticket.AssigneeId != userId;

                bool hasThisTicket = DbContext.Projects
                        .Any(p => p.Users.Any(b => b.Id == userId)
                        && p.Tickets.Any(c => c.Id == ticket.Id)
                        );

                if (isNotCreator && isNotAssignee)
                {
                    if (!hasThisTicket)
                    {
                        if (isSubmitter)
                        {
                            return nameof(TicketController.IndexSubAll);
                        }

                        if (isDeveloper)
                        {
                            return nameof(TicketController.IndexDevAll);
                        }
                    }

                }
            }

            return null;
        }

        private CreateEditTicketViewModel MStateNotValid(int? tkId)
        {
            var thisUserId = User.Identity.GetUserId();
            var model = new CreateEditTicketViewModel();


            model.TicketType = new SelectList(DbContext.TicketTypes, "Id", "Name");
            model.TicketPriority = new SelectList(DbContext.TicketPriorities, "Id", "Name");

            if (!IsAdmin() || !IsProjectManager())
            {
                var holdPjts = DbContext.Projects.Where(p => p.Users.Any(m => m.Id == thisUserId)).ToList();
                model.ProjectBelong = new SelectList(holdPjts, "Id", "Name");
            }
            else
            {
                var allPjts = DbContext.Projects.ToList();
                model.ProjectBelong = new SelectList(allPjts, "Id", "Name");
                model.TicketStatus = new SelectList(DbContext.TicketStatuses, "Id", "Name");

            }

            if (tkId.HasValue)
            {
                var thisTicket = DbContext.Tickets.FirstOrDefault(
                                    p => p.Id == tkId);
                if (IsAdmin() || IsProjectManager())
                {
                    model.GetTicketStatus = Convert.ToString(thisTicket.TicketStatusId);
                }
                model.GetProjectBelong = Convert.ToString(thisTicket.ProjectId);
                model.GetTicketType = Convert.ToString(thisTicket.TicketTypeId);
                model.GetTicketPriority = Convert.ToString(thisTicket.TicketPriorityId);
            }

            return model;
        }



        private DetailTicketViewModel MStateNotValidDetail(Ticket ticket, string theUserId)
        {

            bool isNotCreator = ticket.CreatorId != theUserId;
            bool isNotAssignee = ticket.AssigneeId != theUserId;

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
            model.AreYouOwner = true;

            if (!IsAdmin() && !IsProjectManager())
            {
                if (isNotCreator && isNotAssignee)
                {
                    model.AreYouOwner = false;
                }
            }

            return model;


        }










    }
}