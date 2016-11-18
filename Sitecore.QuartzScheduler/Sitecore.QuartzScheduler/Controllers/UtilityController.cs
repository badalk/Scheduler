using Quartz;
using System;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Sitecore.QuartzScheduler.Controllers
{
    public class UtilityController : Controller
    {

        public ActionResult IsValidJobType(string JobType)
        {
            var returnValue = new JsonResult();
            returnValue.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                //string type = "Scheduler.SyncProductReviewsJob, Scheduler";
                bool isValidJobType = false;
                if (Type.GetType(JobType, true, true) != null)
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

        public void ExecuteJob(string JobKey, string Group)
        {
            try
            {
                JobManager jm = new JobManager();
                Diagnostics.Log.Info(String.Format("Job {0} was executed at {1} on demand by {2}", JobKey, DateTime.Now, Context.User.Name), this);
                jm.ExecuteJob(JobKey, Group);
            }
            catch(Exception ex)
            {
                Diagnostics.Log.Error(String.Format("Error occured while exedcuting job {0} at {1} on demand by {2}", JobKey, DateTime.Now, Context.User.Name), this);
                Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
        }
    }
}
