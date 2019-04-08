namespace BugTracker.Migrations
{
    using BugTracker.Models;
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







        }
    }
}
