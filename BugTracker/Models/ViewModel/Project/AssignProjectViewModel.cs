using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel.Project
{
    public class AssignProjectViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        public AssignProjectViewModel()
        {
            MyUsers= new List<UserProjectViewModel>();
            Users = new List<UserProjectViewModel>();
        }

        public virtual List<UserProjectViewModel> Users { get; }
        public virtual List<UserProjectViewModel> MyUsers { get; }

    }
}