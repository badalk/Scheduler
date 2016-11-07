using Quartz;
using System;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Sitecore.QuartzScheduler.Controllers
{
    public class UtilityController : Controller
    {
        [HttpGet]
        [ActionName("IsValidJobType")]
        //public JsonResult IsValidJobType()
        //{
        //    return new JsonResult() { Data = "Is this is a valid Job Type", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}
        public ActionResult IsValidJobType(string JobType)
        {
            var returnValue = new JsonResult();
            returnValue.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                //string type = "Scheduler.SyncProductReviewsJob, Scheduler";
                bool isValidJobType = false;
                if (Type.GetType(JobType, false, true) != null)
                    isValidJobType = true;


                returnValue.Data = isValidJobType;


            }
            catch (Exception ex)
            {
                Diagnostics.Log.Warn(ex.Message, this);
                returnValue.Data = false;
            }

            return returnValue;

        }

        public ActionResult IsValidCronExpression(string cronExpression)
        {
            var returnValue = new JsonResult();
            returnValue.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            returnValue.Data = CronExpression.IsValidExpression(cronExpression);

            return returnValue;
        }
    }
}
