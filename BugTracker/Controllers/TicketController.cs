using AutoMapper;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Helper;
using BugTracker.Models.ViewModel.Project;
using BugTracker.Models.ViewModel.Ticket;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using static BugTracker.Models.Domain.TicketEnum;

namespace BugTracker.Controllers
{
    [Authorize(Roles = "Admin, Project Manager, Submitter, Developer")]
    public class TicketController : Controller
    {
        private ApplicationDbContext DbContext;
        private readonly UserRoleHelper UserRoleHelper;
        private readonly AppHepler AppHepler;

        public TicketController()
        {
            DbContext = new ApplicationDbContext();
            UserRoleHelper = new UserRoleHelper(DbContext);
            AppHepler = new AppHepler(DbContext);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Index()
        {
            var allTickets = AppHepler.GetAllTickets();
            var appUserId = User.Identity.GetUserId();

            var model = ViewModelIndexTicket(allTickets);

            return View(model);
        }

        [Authorize(Roles = "Submitter")]
        public ActionResult IndexSubCreated()
        {
            var appUserId = User.Identity.GetUserId();
            var currentUser = AppHepler.GetUserById(appUserId);

            var model = currentUser.CreatedTickets
                .Where(p => p.Project.Archived == false)
                .Select(b => new IndexTicketViewModel
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

            var currentUserDisplayName = AppHepler
                    .GetDisplayName4CurrentUserById(appUserId);

            var allTickets = AppHepler
                .GetAllTicketsForDevOrSub(AppHepler.WhichRole.Submitter, appUserId);

            var model = ViewModelIndexTicket(allTickets);


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

            var ticketAssigned = AppHepler.GetUserById(appUserId)
                .AssignedTickets
                .Where(p => p.Project.Archived == false)
                .ToList();

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

            var currentUserDisplayName = AppHepler
                .GetDisplayName4CurrentUserById(appUserId);

            var allTickets = AppHepler
                .GetAllTicketsForDevOrSub(AppHepler.WhichRole.Developer, appUserId);

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
            var subPjts = AppHepler.GetProjects4CurrentUserById(appUserId);

            var model = new CreateEditTicketViewModel();
            model.ProjectBelong = new SelectList(subPjts, "Id", "Name");
            model.TicketType = new SelectList(DbContext.TicketTypes, "Id", "Name");
            model.TicketPriority = new SelectList(DbContext.TicketPriorities, "Id", "Name");
            model.GetTicketStatus = Convert.ToString(AppHepler.GetDefaultTicketStatus());
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
                ticketForSaving.TicketStatusId = AppHepler.GetDefaultTicketStatus();
                DbContext.Tickets.Add(ticketForSaving);

            }
            else
            {
                ticketForSaving = AppHepler.GetTicketById(id);

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

                    var statusChanged = ChangeTeller(ticketForSaving, formData, PrevValue.TicketStatusId);

                    CreateHistory(ticketForSaving, HistoryProperty.TicketStatus,
                        statusChanged, appUserId,
                        ticketForSaving.TicketStatusId.ToString(), formData.GetTicketStatus);

                    ticketForSaving.TicketStatusId = Convert.ToInt32(formData.GetTicketStatus);

                }

                var subject = "Contents";
                var body = $"One or more {subject} is/are edited";

                SendEmail(ticketForSaving, appUserId, subject, body);

            }

            if (id.HasValue)
            {
                var titleChanged = ChangeTeller(ticketForSaving, formData, PrevValue.Title);

                CreateHistory(ticketForSaving, HistoryProperty.TicketTitle,
                    titleChanged, appUserId,
                    ticketForSaving.Title, formData.Title);

                var descriptionChanged = ChangeTeller(ticketForSaving, formData, PrevValue.Description);

                CreateHistory(ticketForSaving, HistoryProperty.TicketDescription,
                    descriptionChanged, appUserId,
                    ticketForSaving.Description, formData.Description);

                var projectChanged = ChangeTeller(ticketForSaving, formData, PrevValue.ProjectId);

                CreateHistory(ticketForSaving, HistoryProperty.ProjectBelong,
                    projectChanged, appUserId,
                    ticketForSaving.ProjectId.ToString(), formData.GetProjectBelong);

                var ticketPriorityChanged = ChangeTeller(ticketForSaving, formData, PrevValue.TicketPriorityId);

                CreateHistory(ticketForSaving, HistoryProperty.TicketPriority,
                    ticketPriorityChanged, appUserId,
                    ticketForSaving.TicketPriorityId.ToString(), formData.GetTicketPriority);

                var ticketTypeChanged = ChangeTeller(ticketForSaving, formData, PrevValue.TicketTypeId);

                CreateHistory(ticketForSaving, HistoryProperty.TicketType,
                    ticketTypeChanged, appUserId,
                    ticketForSaving.TicketTypeId.ToString(), formData.GetTicketType);


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

            var ticket = AppHepler.GetTicketById(id);

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


            var ownedPjts = AppHepler.GetProjects4CurrentUserById(appUserId);

            var model = new CreateEditTicketViewModel();
            model.Title = ticket.Title;
            model.Description = ticket.Description;
            model.TicketType = new SelectList(DbContext.TicketTypes, "Id", "Name");
            model.TicketPriority = new SelectList(DbContext.TicketPriorities, "Id", "Name");
            model.GetTicketPriority = Convert.ToString(ticket.TicketPriorityId);
            model.GetTicketType = Convert.ToString(ticket.TicketTypeId);

            if (IsAdmin() || IsProjectManager())
            {
                var allPjts = AppHepler.GetAllProjects();
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

            var ticket = AppHepler.GetTicketById(id);

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

            var ticket = AppHepler.GetTicketById(tkId);
            var user = AppHepler.GetUserById(userId);

            bool freshTicket = true;
            if (user.AssignedTickets.Any())
            {
                freshTicket = user.AssignedTickets.Any(p => p.Id != ticket.Id);
            }
            var appUserId = User.Identity.GetUserId();

            if (freshTicket)
            {
                var assigneeChanged = ticket.AssigneeId != userId;

                var assigneeName = "None";

                if (ticket.AssigneeId != null)
                {
                    assigneeName = ticket.Assignee.DisplayName;

                    if (assigneeChanged)
                    {
                        AddDelNotification(false, tkId, ticket.AssigneeId);
                        var subject2 = "Assignee";
                        var body2 = $"{ticket.Assignee.DisplayName} are removed from it.";

                        try
                        {
                            SendEmail(ticket, appUserId, subject2, body2);
                        }
                        catch (Exception em)
                        {
                            ViewBag.ErroMsg = $"{em.Message}";
                            return View("ErroMsg");
                        }

                    }

                }

                CreateHistory(ticket, HistoryProperty.Assignee,
                    assigneeChanged, appUserId,
                    assigneeName, user.DisplayName);

                user.AssignedTickets.Add(ticket);

                AddDelNotification(true, tkId, userId);

                var subject = "Assignee";
                var body = $"{subject} is now: {user.DisplayName}.";

                try
                {
                    SendEmail(ticket, appUserId, subject, body);
                }
                catch (Exception em)
                {
                    ViewBag.ErroMsg = $"{em.Message}";
                    return View("ErroMsg");
                }





                DbContext.SaveChanges();
            }

            return RedirectToAction(nameof(TicketController.AssignTicketManagement), new { id = tkId });
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult UnAssign(int tkId, string userId)
        {
            var ticket = AppHepler.GetTicketById(tkId);
            var user = AppHepler.GetUserById(userId);

            bool includedTicket = user.AssignedTickets.Any(p => p.Id == ticket.Id);
            if (includedTicket)
            {
                var appUserId = User.Identity.GetUserId();
                var assigneeChanged = true;

                CreateHistory(ticket, HistoryProperty.Assignee,
                    assigneeChanged, appUserId,
                    ticket.Assignee.DisplayName, "None");

                user.AssignedTickets.Remove(ticket);
                DbContext.SaveChanges();

                var existedNoteId = DbContext.TicketNotifications
                        .Where(p => p.TicketId == tkId && p.UserId == userId)
                        .Select(b => b.Id).FirstOrDefault();
                if (existedNoteId != 0)
                {
                    var subject = "Assignee";
                    var body = $"You are removed from it.";

                    SendEmail(ticket, appUserId, subject, body);

                    AddDelNotification(false, tkId, userId);
                }
            }

            return RedirectToAction(nameof(TicketController.AssignTicketManagement), new { id = tkId });
        }

        [HttpGet]
        public ActionResult Detail(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction(nameof(TicketController.Index));
            var appUserId = User.Identity.GetUserId();

            var ticket = AppHepler.GetTicketById(id);

            if (ticket == null)
            {
                if (IsAdmin() || IsProjectManager())
                {
                    return RedirectToAction(nameof(TicketController.Index));
                }
                else if (IsSubmitter())
                {
                    return RedirectToAction(nameof(TicketController.IndexSubAll));
                }
                else if (IsDeveloper())
                {
                    return RedirectToAction(nameof(TicketController.IndexDevAll));
                }

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

            var model = MStateNotValidDetail(ticket, appUserId);

            return View(model);

        }

        private List<IndexTicketViewModel> ViewModelIndexTicket(List<Ticket> tickets)
        {
            return tickets.Select(b => new IndexTicketViewModel
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
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult OptIn(int tkId)
        {
            var appUserId = User.Identity.GetUserId();
            AddDelNotification(true, tkId, appUserId);

            var ticket = AppHepler.GetTicketById(tkId);
            var model = MStateNotValidDetail(ticket, appUserId);
            //var allTickets = AppHepler.GetAllTickets();
            //var model = ViewModelIndexTicket(allTickets);

            return View(nameof(TicketController.Detail), model);
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult OptOut(int tkId)
        {
            var appUserId = User.Identity.GetUserId();
            AddDelNotification(false, tkId, appUserId);

            var ticket = AppHepler.GetTicketById(tkId);
            var model = MStateNotValidDetail(ticket, appUserId);

            //var allTickets = AppHepler.GetAllTickets();
            //var model = ViewModelIndexTicket(allTickets);

            return View(nameof(TicketController.Detail), model);

        }

        [HttpPost]
        public ActionResult Attachment(int id, AttachmentTicketViewModel formData)
        {
            var ticket = AppHepler.GetTicketById(id);
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
                //// Check size
                var actualSize = Convert.ToInt32(formData.Media.ContentLength);
                var ceiling = 2097152;
                var isOverLimit = actualSize.CompareTo(ceiling);

                if (isOverLimit > 0)
                {
                    ModelState.AddModelError("", "File is too big, must smaller than 2 MB.");
                    return View("Detail", MStateNotValidDetail(ticket, appUserId));
                }


                //// Validating file upload
                var fileExtensionForSaving = Path.GetExtension(formData.Media.FileName).ToLower();

                if (!AttachmentHandler.AllowedFileExtensions.Contains(fileExtensionForSaving))
                {
                    ModelState.AddModelError("", "File extension is not allowed.");
                    return View("Detail", MStateNotValidDetail(ticket, appUserId));
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
                    Description = formData.Description,

                };

                DbContext.TicketAttachments.Add(uploadFile);
                DbContext.SaveChanges();

                var subject = "New Attachment";
                var body = $"{subject} is added";

                SendEmail(ticket, appUserId, subject, body);

            }

            return RedirectToAction(nameof(TicketController.Detail), new { id });
        }


        [HttpPost]
        public ActionResult Comment(int id, CommentTicketViewModel formData)
        {
            var ticket = AppHepler.GetTicketById(id);
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
                DateCreated = DateTime.Now,
            };

            DbContext.TicketComments.Add(freshComment);

            DbContext.SaveChanges();

            var subject = "New Comment";
            var body = $"{subject} is added";

            SendEmail(ticket, appUserId, subject, body);

            return RedirectToAction(nameof(TicketController.Detail), new { id });
        }

        [HttpPost]
        public ActionResult DeleteComment(int? id, int tkId)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketController.Detail), new { tkId });
            }
            var commentToDel = AppHepler.GetTicketCommentById(id);
            var appUserId = User.Identity.GetUserId();

            var redirectLink = RedirectToAction(nameof(TicketController.Detail), new { id = tkId });

            // Check OwnerShip
            var result = OwnershipCheckCommentNew(appUserId, commentToDel);

            if (result == nameof(TicketController.Detail))
            {
                return redirectLink;
            }

            if (commentToDel != null)
            {
                DbContext.TicketComments.Remove(commentToDel);
                DbContext.SaveChanges();
            }

            return redirectLink;
        }

        [HttpPost]
        public ActionResult DeleteAttachment(int? id, int tkId)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketController.Detail), new { tkId });
            }
            var attachmentToDel = AppHepler.GetTicketAttachmentById(id);

            var appUserId = User.Identity.GetUserId();

            var redirectLink = RedirectToAction(nameof(TicketController.Detail), new { id = tkId });

            // Check OwnerShip
            var result = OwnershipCheckAttachmentNew(appUserId, attachmentToDel);

            if (result == nameof(TicketController.Detail))
            {
                return redirectLink;
            }

            if (attachmentToDel != null)
            {
                DbContext.TicketAttachments.Remove(attachmentToDel);
                DbContext.SaveChanges();

                var fileFullUrl = attachmentToDel.FilePath;

                if (System.IO.File.Exists(fileFullUrl))
                {
                    System.IO.File.Delete(fileFullUrl);
                }
            }

            return redirectLink;
        }



        [HttpGet]
        public ActionResult EditComment(int? id)
        {

            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketController.Index));
            }

            var comment = AppHepler.GetTicketCommentById(id);

            if (comment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed);
            }

            var model = new EditCommentTicketViewModel();

            model.Comment = comment.Comment;
            model.TicketId = comment.TicketId;

            return View(model);
        }

        [HttpPost]
        public ActionResult EditComment(int id, EditCommentTicketViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                var comment = AppHepler.GetTicketCommentById(id);

                var model = new EditCommentTicketViewModel();

                model.Comment = comment.Comment;
                model.TicketId = comment.TicketId;

                return View(model);
            }

            var commentForSaving = AppHepler.GetTicketCommentById(id);

            commentForSaving.Comment = formData.Comment;

            if (commentForSaving == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed);
            }

            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketController.Detail), new { id = commentForSaving.TicketId });
        }






        private bool IsAdmin() { return User.IsInRole("Admin"); }
        private bool IsProjectManager() { return User.IsInRole("Project Manager"); }
        private bool IsSubmitter() { return User.IsInRole("Submitter"); }
        private bool IsDeveloper() { return User.IsInRole("Developer"); }

        private string OwnershipCheckEdit(string userId, Ticket ticket)
        {
            bool isAdmin = IsAdmin();
            bool isProjm = IsProjectManager();

            if (!isAdmin && !isProjm)
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



        private string OwnershipCheckCommentNew(string userId, TicketComment comment)
        {

            bool isNotCreator = comment.CreatorId != userId;
            return LogicCommentAttachment(isNotCreator);

        }

        private string OwnershipCheckAttachmentNew(string userId, TicketAttachment attachment)
        {

            bool isNotCreator = attachment.CreatorId != userId;
            return LogicCommentAttachment(isNotCreator);

        }



        private string LogicCommentAttachment(bool tf)
        {
            bool isAdmin = IsAdmin();
            bool isProjm = IsProjectManager();

            if (!isAdmin && !isProjm)
            {
                bool isSubmitter = IsSubmitter();
                bool isDeveloper = IsDeveloper();

                if (tf)
                {
                    return nameof(TicketController.Detail);
                }
            }
            return null;
        }

        private string OwnershipCheckDetail(string userId, Ticket ticket)
        {

            bool isAdmin = IsAdmin();
            bool isProjm = IsProjectManager();

            if (!isAdmin && !isProjm)
            {
                bool isSubmitter = IsSubmitter();
                bool isDeveloper = IsDeveloper();

                bool isNotCreator = ticket.CreatorId != userId;
                bool isNotAssignee = ticket.AssigneeId != userId;

                bool hasThisTicket = DbContext.Projects
                        .Any(p => p.Archived == false
                        && p.Users.Any(b => b.Id == userId)
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
                var holdPjts = AppHepler.GetProjects4CurrentUserById(thisUserId);
                model.ProjectBelong = new SelectList(holdPjts, "Id", "Name");
            }
            else
            {
                var allPjts = AppHepler.GetAllProjects();
                model.ProjectBelong = new SelectList(allPjts, "Id", "Name");
                model.TicketStatus = new SelectList(DbContext.TicketStatuses, "Id", "Name");

            }

            if (tkId.HasValue)
            {
                var thisTicket = AppHepler.GetTicketById(tkId);
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
            model.TicketAttachments = Mapper.Map<List<AttachmentDetailViewModel>>(ticket.TicketAttachments);
            model.TicketComments = Mapper.Map<List<CommentDetailViewModel>>(ticket.TicketComments);
            model.TicketHistories = Mapper.Map<List<HistoryDetailViewModel>>(ticket.TicketHistories);
            model.AreYouOwner = true;
            model.Subscription = ticket.TicketNotifications
                        .Any(p => p.UserId == theUserId);

            if (!IsAdmin() && !IsProjectManager())
            {
                if (isNotCreator && isNotAssignee)
                {
                    model.AreYouOwner = false;
                }

                foreach (var a in model.TicketComments)
                {
                    a.OwnerComment = a.CreatorId == theUserId;
                }

                foreach (var a in model.TicketAttachments)
                {
                    a.OwnerAttachment = a.CreatorId == theUserId;
                }
            }

            return model;


        }

        private bool ChangeTeller(Ticket ticket, CreateEditTicketViewModel data, PrevValue prevInput)
        {
            ////reflection
            //ticket.GetType().GetProperties();

            if (prevInput == PrevValue.Title)
            {
                return ticket.Title != data.Title;
            }
            else if (prevInput == PrevValue.Description)
            {
                return ticket.Description != data.Description;
            }
            else if (prevInput == PrevValue.ProjectId)
            {
                return ticket.ProjectId != Convert.ToInt32(data.GetProjectBelong);
            }
            else if (prevInput == PrevValue.TicketPriorityId)
            {
                return ticket.TicketPriorityId != Convert.ToInt32(data.GetTicketPriority);
            }
            else if (prevInput == PrevValue.TicketTypeId)
            {
                return ticket.TicketTypeId != Convert.ToInt32(data.GetTicketType);
            }
            else if (prevInput == PrevValue.TicketStatusId)
            {
                return ticket.TicketStatusId != Convert.ToInt32(data.GetTicketStatus);
            }

            return false;
        }

        private void CreateHistory(Ticket ticket, HistoryProperty historyProperty,
                    bool changeDetected, string userId, string oldValue, string newValue)
        {
            if (changeDetected)
            {
                var freshHistory = new TicketHistory()
                {
                    TicketId = ticket.Id,
                    ModifierId = userId,
                    Property = historyProperty.ToString(),
                    TimeStamp = DateTime.Now,
                };

                if (historyProperty == HistoryProperty.ProjectBelong)
                {
                    var realOldValue = ticket.Project.Name;
                    var inputId = Convert.ToInt32(newValue);
                    var realNewValue = AppHepler.GetProjectById(inputId).Name;

                    AddValueHistory(realOldValue, realNewValue, freshHistory);

                }
                else if (historyProperty == HistoryProperty.TicketDescription)
                {
                    AddValueHistory(oldValue, newValue, freshHistory);
                }
                else if (historyProperty == HistoryProperty.TicketPriority)
                {
                    var realOldValue = ticket.TicketPriority.Name;
                    var inputId = Convert.ToInt32(newValue);
                    var realNewValue = AppHepler.GetTkPriorityById(inputId).Name;
                    AddValueHistory(realOldValue, realNewValue, freshHistory);
                }
                else if (historyProperty == HistoryProperty.TicketStatus)
                {
                    var realOldValue = ticket.TicketStatus.Name;
                    var inputId = Convert.ToInt32(newValue);
                    var realNewValue = AppHepler.GetTkStatusById(inputId).Name;
                    AddValueHistory(realOldValue, realNewValue, freshHistory);
                }
                else if (historyProperty == HistoryProperty.TicketType)
                {
                    var realOldValue = ticket.TicketType.Name;
                    var inputId = Convert.ToInt32(newValue);
                    var realNewValue = AppHepler.GetTkTypeyById(inputId).Name;
                    AddValueHistory(realOldValue, realNewValue, freshHistory);

                }
                else if (historyProperty == HistoryProperty.TicketTitle)
                {
                    AddValueHistory(oldValue, newValue, freshHistory);
                }
                else if (historyProperty == HistoryProperty.Assignee)
                {
                    AddValueHistory(oldValue, newValue, freshHistory);
                }

                DbContext.TicketHistories.Add(freshHistory);

                DbContext.SaveChanges();
            }
        }

        private void AddValueHistory(string realOldValue, string realNewValue, TicketHistory freshHistory)
        {

            freshHistory.OldValue = realOldValue;

            if (realNewValue != null)
            {
                freshHistory.NewValue = realNewValue;
            }

        }

        private void AddDelNotification(bool addOrDel, int tkId, string userId)
        {
            if (addOrDel)
            {
                bool existedNote = DbContext.TicketNotifications
                    .Any(p => p.TicketId == tkId && p.UserId == userId);

                if (!existedNote)
                {
                    var freshNote = new TicketNotification()
                    {
                        TicketId = tkId,
                        UserId = userId,
                    };

                    DbContext.TicketNotifications.Add(freshNote);

                }

            }
            else
            {
                var noteDel = DbContext.TicketNotifications
                        .Where(p => p.TicketId == tkId && p.UserId == userId)
                        .FirstOrDefault();
                if (noteDel != null)
                {
                    DbContext.TicketNotifications.Remove(noteDel);
                }

            }


            DbContext.SaveChanges();

        }

        private void SendEmail(Ticket ticket, string modifierId, string title, string message)
        {

            if ((IsAdmin() || IsProjectManager())
                    || ticket.AssigneeId != null)
            {
                var eService = new EmailService();

                var email = ticket.TicketNotifications.Where(b => b.UserId != modifierId)
                    .Select(p => p.User.Email).ToList();

                if (email.Any())
                {
                    var subject = $"Changes on {title} of Ticket-{ticket.Id}";
                    var body = $"Ticket: {ticket.Title}, {message}";

                    eService.SendMuti(email, subject, body);
                }
            }


        }








    }
}