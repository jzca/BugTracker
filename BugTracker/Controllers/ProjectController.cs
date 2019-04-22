using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Helper;
using BugTracker.Models.ViewModel.Project;
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
        private readonly UserRoleHelper UserRoleHelper;

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
                    DateUpdated = p.DateUpdated,
                    AssignedUsers = p.Users.Count,
                    Tickets = p.Tickets.Count,
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
                    DateUpdated = p.DateUpdated,
                    AssignedUsers = p.Users.Count,
                    Tickets = p.Tickets.Count,
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

            var repeated = new UserProjectViewModel();

            foreach(var userRm in project.Users)
            {
                repeated = allUsers.Where(m => m.Id == userRm.Id).FirstOrDefault();
                allUsers.Remove(repeated);
            }

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

            var leftUsers = DbContext.Users
                .Where(p=> p.Id != userId)
                .Select(n => new UserProjectViewModel
            {
                Id = n.Id,
                UserName = n.UserName
            }).ToList();


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

            //var model = new AssignProjectViewModel();

            //model.ProjectId = project.Id;
            //model.ProjectName = project.Name;
            //model.MyUsers.AddRange(project.Users
            //    .Select(n => new UserProjectViewModel
            //    {
            //        Id = n.Id,
            //        UserName = n.UserName
            //    }).ToList());
            //model.Users.AddRange(leftUsers);

            //return View("AssignProjectManagement", model);

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



    }
}