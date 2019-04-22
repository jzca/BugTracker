using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModel.Role
{
    public class UserForAssignRoleViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string DisplayIsAdmin { get; set; }
        public string DisplayIsPm { get; set; }
        public string DisplayIsDev { get; set; }
        public string DisplayIsSub { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsPm { get; set; }
        public bool IsDev { get; set; }
        public bool IsSub { get; set; }
    }
}