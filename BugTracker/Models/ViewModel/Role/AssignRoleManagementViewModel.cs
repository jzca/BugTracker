using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel.Role
{
    public class AssignRoleManagementViewModel
    {
        public AssignRoleManagementViewModel()
        {

            Users = new List<UserForAssignRoleViewModel>();
            Roles = new List<string>();
        }

        public virtual List<UserForAssignRoleViewModel> Users { get; }
        public virtual List<string> Roles { get; }
    }
}