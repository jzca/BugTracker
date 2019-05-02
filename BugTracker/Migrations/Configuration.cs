namespace BugTracker.Migrations
{
    using BugTracker.Models;
    using BugTracker.Models.Domain;
    using BugTracker.Models.Helper;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        private UserRoleHelper UserRoleHelper { get; set; }

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            //UserRoleHelper = new UserRoleHelper(context);
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {

            context.TicketPriorities.AddOrUpdate(p => p.Name,
                            new TicketPriority { Name = TicketEnum.Priority.Low.ToString() },
                            new TicketPriority { Name = TicketEnum.Priority.Medium.ToString() },
                            new TicketPriority { Name = TicketEnum.Priority.High.ToString() }
                            );

            context.TicketStatuses.AddOrUpdate(p => p.Name,
                        new TicketStatus { Name = TicketEnum.Status.Open.ToString() },
                        new TicketStatus { Name = TicketEnum.Status.Rejected.ToString() },
                        new TicketStatus { Name = TicketEnum.Status.Resolved.ToString() }
                        );

            context.TicketTypes.AddOrUpdate(p => p.Name,
                        new TicketType { Name = TicketEnum.Type.Bug.ToString() },
                        new TicketType { Name = TicketEnum.Type.Database.ToString() },
                        new TicketType { Name = TicketEnum.Type.Feature.ToString() },
                        new TicketType { Name = TicketEnum.Type.Support.ToString() }
                        );

            //Seeding Users and Roles

            //RoleManager, used to manage roles
            var roleManager =
                new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(context));

            //UserManager, used to manage users
            var userManager =
                new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(context));

            //Adding admin role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == "Admin"))
            {
                var adminRole = new IdentityRole("Admin");
                roleManager.Create(adminRole);
            }

            //Adding Project Manager role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == "Project Manager"))
            {
                var pmngRole = new IdentityRole("Project Manager");
                roleManager.Create(pmngRole);
            }
            //Adding Developer role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == "Developer"))
            {
                var deveRole = new IdentityRole("Developer");
                roleManager.Create(deveRole);
            }
            //Adding Submitter role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == "Submitter"))
            {
                var submRole = new IdentityRole("Submitter");
                roleManager.Create(submRole);
            }

            //Creating the adminuser
            ApplicationUser adminUser;

            if (!context.Users.Any(
                p => p.UserName == "admin@mybugtracker.com"))
            {
                adminUser = new ApplicationUser();
                adminUser.UserName = "admin@mybugtracker.com";
                adminUser.Email = "admin@mybugtracker.com";
                adminUser.DisplayName = adminUser.Email.Split('@')[0];

                userManager.Create(adminUser, "Password-1");
            }
            else
            {
                adminUser = context
                    .Users
                    .First(p => p.UserName == "admin@mybugtracker.com");
            }

            //Make sure the user is on the admin role
            if (!userManager.IsInRole(adminUser.Id, "Admin"))
            {
                userManager.AddToRole(adminUser.Id, "Admin");
            }

            //Creating the subUser
            ApplicationUser subUser;

            if (!context.Users.Any(
                p => p.UserName == "sub@mybugtracker.com"))
            {
                subUser = new ApplicationUser();
                subUser.UserName = "sub@mybugtracker.com";
                subUser.Email = "sub@mybugtracker.com";
                subUser.DisplayName = subUser.Email.Split('@')[0];

                userManager.Create(subUser, "Password-1");
            }
            else
            {
                subUser = context
                    .Users
                    .First(p => p.UserName == "sub@mybugtracker.com");
            }

            //Make sure the user is on the sub role
            if (!userManager.IsInRole(subUser.Id, "Submitter"))
            {
                userManager.AddToRole(subUser.Id, "Submitter");
            }

            //Creating the devUser
            ApplicationUser devUser;

            if (!context.Users.Any(
                p => p.UserName == "dev@mybugtracker.com"))
            {
                devUser = new ApplicationUser();
                devUser.UserName = "dev@mybugtracker.com";
                devUser.Email = "dev@mybugtracker.com";
                devUser.DisplayName = devUser.Email.Split('@')[0];

                userManager.Create(devUser, "Password-1");
            }
            else
            {
                devUser = context
                    .Users
                    .First(p => p.UserName == "dev@mybugtracker.com");
            }

            //Make sure the user is on the dev role
            if (!userManager.IsInRole(devUser.Id, "Developer"))
            {
                userManager.AddToRole(devUser.Id, "Developer");
            }

            //Creating the pmUser
            ApplicationUser pmUser;

            if (!context.Users.Any(
                p => p.UserName == "pm@mybugtracker.com"))
            {
                pmUser = new ApplicationUser();
                pmUser.UserName = "pm@mybugtracker.com";
                pmUser.Email = "pm@mybugtracker.com";
                pmUser.DisplayName = pmUser.Email.Split('@')[0];

                userManager.Create(pmUser, "Password-1");
            }
            else
            {
                pmUser = context
                    .Users
                    .First(p => p.UserName == "pm@mybugtracker.com");
            }

            //Make sure the user is on the pm role
            if (!userManager.IsInRole(pmUser.Id, "Project Manager"))
            {
                userManager.AddToRole(pmUser.Id, "Project Manager");
            }

            //Creating the devUser2
            ApplicationUser devUser2;

            if (!context.Users.Any(
                p => p.UserName == "dev2@mybugtracker.com"))
            {
                devUser2 = new ApplicationUser();
                devUser2.UserName = "dev2@mybugtracker.com";
                devUser2.Email = "dev2@mybugtracker.com";
                devUser2.DisplayName = "devTestEmail";

                userManager.Create(devUser2, "Password-1");
            }
            else
            {
                devUser2 = context
                    .Users
                    .First(p => p.UserName == "dev2@mybugtracker.com");
            }

            //Make sure the user is on the dev role
            if (!userManager.IsInRole(devUser2.Id, "Developer"))
            {
                userManager.AddToRole(devUser2.Id, "Developer");
            }






        }
    }
}