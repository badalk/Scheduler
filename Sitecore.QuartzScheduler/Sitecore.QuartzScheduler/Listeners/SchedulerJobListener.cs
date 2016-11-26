using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.QuartzScheduler.Listeners
{
    public class SchedulerJobListener : IJobListener
    {
        public string Name
        {
            get
            {
                return this.GetType().ToString(); 
            }
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            Sitecore.Diagnostics.Log.Warn(String.Format("JobExecutionVetoed for job {0} at {1}", context.JobDetail.Key.Name, DateTime.Now), this);

        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("JobToBeExecuted {0}", context.JobDetail.Key.Name), this);
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("JobWasExecuted {0}", context.JobDetail.Key.Name), this);

            if (jobException != null)
                Sitecore.Diagnostics.Log.Error(String.Format("Sitecore.QuartzScheuler: JobWasExecuted {0} with Error : {1}", 
                                                            context.JobDetail.Key.Name, 
                                                            jobException.Message + Environment.NewLine + jobException.StackTrace), 
                                                this);
        }
    }
}