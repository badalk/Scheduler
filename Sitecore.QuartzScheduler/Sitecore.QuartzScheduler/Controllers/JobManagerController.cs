using Sitecore.QuartzScheduler.Models;
using Sitecore.Services.Core;
using System.Web.Mvc;
using Sitecore.Services.Infrastructure.Sitecore.Services;
using Sitecore.QuartzScheduler.Repository;
using System;

namespace Sitecore.QuartzScheduler.Controllers
{
    [ServicesController]
    public class JobManagerController : EntityService<JobDetail>
    {

        public JobManagerController(IRepository<JobDetail> repository): base(repository)
        {

        }

        public JobManagerController() : this(new JobDetailRepository())
        {
            
        }




    }
}