using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker
{
    public class AttachmentHandler
    {
        //public static readonly List<string> AllowedFileExtensions =
        //    new List<string> { ".jpg", ".jpeg", ".png", "gif" };

        public static readonly string AttachmentSaveFolder = "~/FilesSaved/";

        public static readonly string MappedUploadFolder = HttpContext.Current.Server.MapPath(AttachmentSaveFolder);
    }
}