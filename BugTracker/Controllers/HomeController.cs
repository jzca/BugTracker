using AutoMapper;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filter;
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
    public class HomeController : Controller
    {
        private ApplicationDbContext DbContext;
        private readonly UserRoleHelper UserRoleHelper;

        public HomeController()
        {
            DbContext = new ApplicationDbContext();
            UserRoleHelper = new UserRoleHelper(DbContext);
        }

        private bool IsAdmin() { return User.IsInRole("Admin"); }
        private bool IsProjectManager() { return User.IsInRole("Project Manager"); }
        private bool IsSubmitter() { return User.IsInRole("Submitter"); }
        private bool IsDeveloper() { return User.IsInRole("Developer"); }

        [LogActionFilter]
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }

            bool isAdmin = IsAdmin();
            bool isProjm = IsProjectManager();
            bool isSubmitter = IsSubmitter();
            bool isDeveloper = IsDeveloper();

            var appUserId = User.Identity.GetUserId();

            int ownedProjects = 0;
            int ownedTickets = 0;

            if (!isAdmin && !isProjm)
            {
                ownedProjects = DbContext.Projects
                        .Where(p => p.Archived == false && p.Users.Any(b => b.Id == appUserId))
                        .Count();

                if (isDeveloper)
                {
                    ownedTickets = DbContext.Tickets
                           .Where(p => p.Project.Archived == false && p.AssigneeId == appUserId)
                           .Count();
                }
                else if (isSubmitter)
                {
                    ownedTickets = DbContext.Tickets
                            .Where(p => p.Project.Archived == false && p.CreatorId == appUserId)
                            .Count();
                }


            }

            IndexHomeViewModel model;
            model = new IndexHomeViewModel();


            if (isAdmin || isProjm)
            {
                var numProjects = DbContext.Projects.Where(p => p.Archived == false).Count();
                var tickets = DbContext.Tickets.Where(p => p.Project.Archived == false).ToList();
                var numTickets = tickets.Count();

                var openTks = tickets
                    .Where(p =>
                   p.TicketStatus.Name == TicketEnum.Status.Open.ToString())
                    .Count();

                var rejectedTks = tickets
                    .Where(p =>
                   p.TicketStatus.Name == TicketEnum.Status.Rejected.ToString())
                    .Count();

                var resolvedTks = tickets
                    .Where(p =>
                   p.TicketStatus.Name == TicketEnum.Status.Resolved.ToString())
                    .Count();



                model = new IndexHomeViewModel()
                {
                    YesAdmin = isAdmin,
                    YesDev = isDeveloper,
                    YesPm = isProjm,
                    YesSub = isSubmitter,
                    NumOwnedProjects = numProjects,
                    NumOwnedTickets = numTickets,
                    NumOpenTk = openTks,
                    NumRejectedTk = rejectedTks,
                    NumResolvedTk = resolvedTks,

                };
            }
            else if (isDeveloper)
            {

                model = new IndexHomeViewModel()
                {
                    YesAdmin = isAdmin,
                    YesDev = isDeveloper,
                    YesPm = isProjm,
                    YesSub = isSubmitter,
                    NumOwnedProjects = ownedProjects,
                    NumOwnedTickets = ownedTickets,
                    NumRejectedTk = 0,
                    NumOpenTk = 0,
                    NumResolvedTk = 0,
                };
            }
            else if (isSubmitter)
            {

                model = new IndexHomeViewModel()
                {
                    YesAdmin = isAdmin,
                    YesDev = isDeveloper,
                    YesPm = isProjm,
                    YesSub = isSubmitter,
                    NumOwnedProjects = ownedProjects,
                    NumOwnedTickets = ownedTickets,
                    NumRejectedTk = 0,
                    NumOpenTk = 0,
                    NumResolvedTk = 0,
                };
            }


            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AccessLog()
        {
            var allLogs = DbContext.ActionLogs
                .Where(p => p.ActionName.ToLower() != "accesslog")
                .ToList();

            //allLogs.GroupBy(p => p.ActionName).Count();

            var model = new List<ActionLogViewModel>();
            model = Mapper.Map<List<ActionLogViewModel>>(allLogs);

            return View(model);
        }
    }
}