using Sitecore.QuartzScheduler.Common;
using Sitecore.QuartzScheduler.Models;
using Sitecore.QuartzScheduler.Repository;
using Sitecore.Services.Core;
using Sitecore.Services.Infrastructure.Sitecore.Services;
using System.Web.Http.Results;
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

        public ActionResult GetJobTriggerList(string jobId)
        {
            var returnValue = new JsonResult();
            returnValue.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            JobManager jm = new JobManager();
            var jobDetail = jm.GetJobDetails(jobId);
            var triggerList = jm.GetTriggersForJob(jobDetail);
            var jsonData = HelperUtility.GetJsonSerializedData(triggerList);
            returnValue.Data = jsonData;

            return returnValue;
        }


    }
}