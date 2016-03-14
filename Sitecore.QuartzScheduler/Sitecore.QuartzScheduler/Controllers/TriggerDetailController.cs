using Sitecore.QuartzScheduler.Models;
using Sitecore.QuartzScheduler.Repository;
using Sitecore.Services.Core;
using Sitecore.Services.Infrastructure.Sitecore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sitecore.QuartzScheduler.Controllers
{
    [ServicesController]
    public class TriggerDetailController : EntityService<TriggerDetail>
    {

        public TriggerDetailController(IRepository<TriggerDetail> repository) : base(repository)
        {

        }

        public TriggerDetailController(): this(new TriggerDetailRepository())
        {

        }


    }
}