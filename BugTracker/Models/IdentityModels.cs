﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using BugTracker.Models.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugTracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Projects = new List<Project>();
            CreatedTickets = new List<Ticket>();
            AssignedTickets = new List<Ticket>();
            TicketAttachments = new List<TicketAttachment>();
            TicketHistories = new List<TicketHistory>();
            TicketNotifications = new List<TicketNotification>();
        }

        public string DisplayName { get; set; }
        public virtual List<Project> Projects { get; set; }
        [InverseProperty(nameof(Ticket.Creator))]
        public virtual List<Ticket> CreatedTickets { get; set; }
        [InverseProperty(nameof(Ticket.Assignee))]
        public virtual List<Ticket> AssignedTickets { get; set; }
        public virtual List<TicketAttachment> TicketAttachments { get; set; }
        public virtual List<TicketHistory> TicketHistories { get; set; }
        public virtual List<TicketNotification> TicketNotifications { get; set; }



        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketPriority> TicketPriorities { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<TicketNotification> TicketNotifications { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TicketStatus>()
                .ToTable("TicketStatuses");
                

        }
    }
}