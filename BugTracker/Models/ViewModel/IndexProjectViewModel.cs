using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel
{
    public class IndexProjectViewModel
    {

        //public IndexProjectViewModel()
        //{
        //    Users = new List<ApplicationUser>();
        //}

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        //public virtual List<ApplicationUser> Users { get; }

    }
}