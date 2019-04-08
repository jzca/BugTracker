using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Helper;
using BugTracker.Models.ViewModel;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private ApplicationDbContext DbContext;
        private UserRoleHelper UserRoleHelper;

        public ProjectController()
        {
            DbContext = new ApplicationDbContext();
            UserRoleHelper = new UserRoleHelper(DbContext);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Index()
        {
            var appUserId = User.Identity.GetUserId();

            var model = DbContext.Projects
                .Select(p => new IndexProjectViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated
                }).ToList();

            return View(model);
        }

        public ActionResult IndexIndividual()
        {
            var appUserId = User.Identity.GetUserId();
            var currentUser = DbContext.Users.Where(i => i.Id == appUserId).FirstOrDefault();

            var model = currentUser.Projects
                .Select(p => new IndexProjectViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated
                }).ToList();

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Create(CreateEditProjectViewModel formData)
        {
            return SaveProject(null, formData);
        }

        private ActionResult SaveProject(int? id, CreateEditProjectViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var appUserId = User.Identity.GetUserId();

            Project projectForSavingProject;

            if (!id.HasValue)
            {
                projectForSavingProject = new Project();
                projectForSavingProject.DateCreated = DateTime.Now;

                DbContext.Projects.Add(projectForSavingProject);
            }
            else
            {
                projectForSavingProject = DbContext.Projects.FirstOrDefault(
               p => p.Id == id);

                projectForSavingProject.DateUpdated = DateTime.Now;

                if (projectForSavingProject == null)
                {
                    return RedirectToAction(nameof(ProjectController.Index));
                }
            }

            projectForSavingProject.Name = formData.Name;

            DbContext.SaveChanges();

            return RedirectToAction(nameof(ProjectController.Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(ProjectController.Index));
            }

            var project = DbContext.Projects.FirstOrDefault(
                p => p.Id == id.Value);

            if (project == null)
            {
                return RedirectToAction(nameof(ProjectController.Index));
            }

            var model = new CreateEditProjectViewModel();

            model.Name = project.Name;

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Edit(int id, CreateEditProjectViewModel formData)
        {
            return SaveProject(id, formData);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AssignProjectManagement(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(ProjectController.Index));
            }

            var allUsers = DbContext.Users.Select(n => new UserProjectViewModel
            {
                Id = n.Id,
                UserName = n.UserName
            }).ToList();


            var project = DbContext.Projects.FirstOrDefault(
                p => p.Id == id.Value);

            if (project == null)
            {
                return RedirectToAction(nameof(ProjectController.Index));
            }

            var model = new AssignProjectViewModel();

            model.ProjectId = project.Id;
            model.ProjectName = project.Name;
            model.MyUsers.AddRange(project.Users
                .Select(n => new UserProjectViewModel
                {
                    Id = n.Id,
                    UserName = n.UserName
                }).ToList());

            //if (model.UserNames.Any())
            //{
            //    var listUserNames = model.UserNames.ToList();
            //    foreach (var un in listUserNames.ToList())
            //    {
            //        var nameToRm = project.Users
            //            .Where(p => p.UserName == un)
            //            .Select(n => new UserProjectViewModel
            //            {
            //                Id = n.Id,
            //                UserName = n.UserName
            //            }).FirstOrDefault();
            //        if (nameToRm != null)
            //        {
            //            allUsers.Remove(nameToRm);
            //            listUserNames.Remove(nameToRm.UserName);
            //        }
            //    }
            //}


            model.Users.AddRange(allUsers);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Assign(int pJid, string userId)
        {
            //if (!ModelState.IsValid)
            //{
            //    return RedirectToAction(nameof(ProjectController.AssignManagement), new { id = pJid });
            //}

            var project = DbContext.Projects.FirstOrDefault(p => p.Id == pJid);
            var user = DbContext.Users.FirstOrDefault(p => p.Id == userId);

            //var userRm = DbContext.Users.Where(p => p.Id == userId).Select(n => new UserProjectViewModel
            //{
            //    Id = n.Id,
            //    UserName = n.UserName
            //}).FirstOrDefault();


            bool freshProject = true;
            if (user.Projects.Any())
            {
                freshProject = user.Projects.Any(p => p.Id != project.Id);
            }

            if (freshProject)
            {
                user.Projects.Add(project);
                DbContext.SaveChanges();
            }
            //else
            //{
            //    ModelState.AddModelError(nameof(ProjectController.AssignManagement),
            //    "Already Assigned");
            //}


            return RedirectToAction(nameof(ProjectController.AssignProjectManagement), new { id = pJid });
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult UnAssign(int pJid, string userId)
        {
            var project = DbContext.Projects.FirstOrDefault(p => p.Id == pJid);
            var user = DbContext.Users.FirstOrDefault(p => p.Id == userId);

            bool includedProject = user.Projects.Any(p => p.Id == project.Id);
            if (includedProject)
            {
                user.Projects.Remove(project);
                DbContext.SaveChanges();
            }

            return RedirectToAction(nameof(ProjectController.AssignProjectManagement), new { id = pJid });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AssignRoleManagement()
        {
            var admin = "Admin";
            var dev = "Developer";
            var pjm = "Project Manager";
            var sub = "Submitter";

            var allUsers = DbContext.Users.Select(n => new UserForAssignRoleViewModel
            {
                Id = n.Id,
                UserName = n.UserName
            }).ToList();
            var roles = DbContext.Roles.Select(n => n.Name).ToList();

            foreach (var p in allUsers)
            {
                p.IsAdmin = UserRoleHelper.IsUserInRole(p.Id, admin);
                p.IsDev = UserRoleHelper.IsUserInRole(p.Id, dev);
                p.IsPm = UserRoleHelper.IsUserInRole(p.Id, pjm);
                p.IsSub = UserRoleHelper.IsUserInRole(p.Id, sub);

                p.DisplayIsAdmin = (p.IsAdmin) ? "fas fa-check" : "fas fa-times";
                p.DisplayIsDev = (p.IsDev) ? "fas fa-check" : "fas fa-times";
                p.DisplayIsPm = (p.IsPm) ? "fas fa-check" : "fas fa-times";
                p.DisplayIsSub = (p.IsSub) ? "fas fa-check" : "fas fa-times";


            }


            var model = new AssignRoleManagementViewModel();
            model.Users.AddRange(allUsers);
            model.Roles.AddRange(roles);


            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AssignRole(string id)
        {

            var admin = "Admin";
            var dev = "Developer";
            var pjm = "Project Manager";
            var sub = "Submitter";

            var roles = DbContext.Roles.Select(n => n.Name).ToList();

            var selectedUser = DbContext.Users
                .Where(p => p.Id == id)
                .Select(n => new UserForAssignRoleViewModel
                {
                    Id = n.Id,
                    UserName = n.UserName
                }).FirstOrDefault();

            selectedUser.IsAdmin = UserRoleHelper.IsUserInRole(selectedUser.Id, admin);
            selectedUser.IsDev = UserRoleHelper.IsUserInRole(selectedUser.Id, dev);
            selectedUser.IsPm = UserRoleHelper.IsUserInRole(selectedUser.Id, pjm);
            selectedUser.IsSub = UserRoleHelper.IsUserInRole(selectedUser.Id, sub);

            var model = new AssignRoleViewModel();
            model.User = selectedUser;
            model.Roles.AddRange(roles);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AssignRole(string id, AssignRoleViewModel formData)
        {
            var admin = "Admin";
            var dev = "Developer";
            var pjm = "Project Manager";
            var sub = "Submitter";

            if (formData.User.IsAdmin)
            {
                if (!UserRoleHelper.IsUserInRole(id, admin))
                {
                    UserRoleHelper.AddUserToRole(id, admin);
                }
            }
            else
            {
                if (UserRoleHelper.IsUserInRole(id, admin))
                {
                    UserRoleHelper.RemoveUserFromRole(id, admin);

                }
            }

            if (formData.User.IsDev)
            {
                if (!UserRoleHelper.IsUserInRole(id, dev))
                {
                    UserRoleHelper.AddUserToRole(id, dev);
                }
            }
            else
            {
                if (UserRoleHelper.IsUserInRole(id, dev))
                {
                    UserRoleHelper.RemoveUserFromRole(id, dev);
                }
            }
            if (formData.User.IsPm)
            {
                if (!UserRoleHelper.IsUserInRole(id, pjm))
                {
                    UserRoleHelper.AddUserToRole(id, pjm);
                }
            }
            else
            {
                if (UserRoleHelper.IsUserInRole(id, pjm))
                {
                    UserRoleHelper.RemoveUserFromRole(id, pjm);
                }

            }
            if (formData.User.IsSub)
            {
                if (!UserRoleHelper.IsUserInRole(id, sub))
                {
                    UserRoleHelper.AddUserToRole(id, sub);
                }
            }
            else
            {
                if (UserRoleHelper.IsUserInRole(id, sub))
                {
                    UserRoleHelper.RemoveUserFromRole(id, sub);
                }
            }

            var allUsers = DbContext.Users.Select(n => new UserForAssignRoleViewModel
            {
                Id = n.Id,
                UserName = n.UserName
            }).ToList();

            var roles = DbContext.Roles.Select(n => n.Name).ToList();

            foreach (var p in allUsers)
            {
                p.IsAdmin = UserRoleHelper.IsUserInRole(p.Id, admin);
                p.IsDev = UserRoleHelper.IsUserInRole(p.Id, dev);
                p.IsPm = UserRoleHelper.IsUserInRole(p.Id, pjm);
                p.IsSub = UserRoleHelper.IsUserInRole(p.Id, sub);

                p.DisplayIsAdmin = (p.IsAdmin) ? "fas fa-check" : "fas fa-times";
                p.DisplayIsDev = (p.IsDev) ? "fas fa-check" : "fas fa-times";
                p.DisplayIsPm = (p.IsPm) ? "fas fa-check" : "fas fa-times";
                p.DisplayIsSub = (p.IsSub) ? "fas fa-check" : "fas fa-times";


            }

            var model = new AssignRoleManagementViewModel();
            model.Users.AddRange(allUsers);
            model.Roles.AddRange(roles);

            return View("AssignRoleManagement", model);
        }
    }
}