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


        public ActionResult Index()
        {
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
                        .Where(p => p.Users.Any(b => b.Id == appUserId))
                        .Count();

                if (isDeveloper)
                {
                    ownedTickets = DbContext.Tickets
                           .Where(p => p.AssigneeId == appUserId)
                           .Count();
                }
                else if (isSubmitter)
                {
                    ownedTickets = DbContext.Tickets
                            .Where(p => p.CreatorId == appUserId)
                            .Count();
                }


            }

            IndexHomeViewModel model;
            model = new IndexHomeViewModel();


            if (isAdmin || isProjm)
            {
                var numProjects = DbContext.Projects.Count();
                var tickets = DbContext.Tickets.ToList();
                var numTickets = tickets.Count();

                var openTks = tickets
                    .Where(p =>
                   p.TicketStatus.Name== TicketEnum.Status.Open.ToString())
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
                    NumOwnedTickets=numTickets,
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

    }
}