using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel
{
    public class AssignRoleViewModel
    {
        public AssignRoleViewModel()
        {
            Roles = new List<string>();
        }


        public UserForAssignRoleViewModel User { get; set; }
        public virtual List<string> Roles { get; }
    }
}