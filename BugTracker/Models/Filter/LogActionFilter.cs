using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.Filter
{
    public class LogActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var context = new ApplicationDbContext();

            var log = new ActionLog()
            {

                ActionName = filterContext
                .ActionDescriptor
                .ActionName,

                ControllerName = filterContext
                .ActionDescriptor
                .ControllerDescriptor
                .ControllerName,

                AccessTime = DateTime.Now,

            };


            context.ActionLogs.Add(log);
            context.SaveChanges();

        }

    }



}
