using BugTracker.Models;
using BugTracker.Models.Helper;
using BugTracker.Models.ViewModel.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private ApplicationDbContext DbContext;
        private UserRoleHelper UserRoleHelper;

        public RoleController()
        {
            DbContext = new ApplicationDbContext();
            UserRoleHelper = new UserRoleHelper(DbContext);
        }



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
        public ActionResult AssignRole(string id, AssignRoleViewModel formData)
        {
            var admin = "Admin";
            var dev = "Developer";
            var pjm = "Project Manager";
            var sub = "Submitter";

            var checkIfAdmin = UserRoleHelper.IsUserInRole(id, admin);
            var checkIfDev = UserRoleHelper.IsUserInRole(id, dev);
            var checkIfPm = UserRoleHelper.IsUserInRole(id, pjm);
            var checkIfSub = UserRoleHelper.IsUserInRole(id, sub);

            if (formData.User.IsAdmin)
            {
                if (!checkIfAdmin)
                {
                    UserRoleHelper.AddUserToRole(id, admin);
                }
            }
            else
            {
                if (checkIfAdmin)
                {
                    UserRoleHelper.RemoveUserFromRole(id, admin);

                }
            }

            if (formData.User.IsDev)
            {
                if (!checkIfDev)
                {
                    UserRoleHelper.AddUserToRole(id, dev);
                }
            }
            else
            {
                if (checkIfDev)
                {
                    UserRoleHelper.RemoveUserFromRole(id, dev);
                }
            }
            if (formData.User.IsPm)
            {
                if (!checkIfPm)
                {
                    UserRoleHelper.AddUserToRole(id, pjm);
                }
            }
            else
            {
                if (checkIfPm)
                {
                    UserRoleHelper.RemoveUserFromRole(id, pjm);
                }

            }
            if (formData.User.IsSub)
            {
                if (!checkIfSub)
                {
                    UserRoleHelper.AddUserToRole(id, sub);
                }
            }
            else
            {
                if (checkIfSub)
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