using Sitecore.QuartzScheduler.Models;
using Sitecore.Services.Core;
using System.Web.Mvc;
using Sitecore.Services.Infrastructure.Sitecore.Services;
using Sitecore.QuartzScheduler.Repository;

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

        
        //// GET: JobManager
        //public ActionResult Index()
        //{
        //    return View();
        //}


        //public JsonResult GetAllJobs()
        //{
        //    var jobManager = new JobManager();
        //    var joblist = jobManager.GetAllJobs();
        //    return Json(joblist, JsonRequestBehavior.AllowGet);
        //}
    }
}