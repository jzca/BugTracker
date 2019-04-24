using AutoMapper;
using BugTracker.Models.Domain;
using BugTracker.Models.ViewModel.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.App_Start
{
    public class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<TicketAttachment, AttachmentDetailViewModel>();
                cfg.CreateMap<TicketComment, CommentDetailViewModel>();
            });
        }

    }
}