using Quartz;
using System;
using Sitecore.Diagnostics;


namespace Sitecore.QuartzScheduler.Jobs
{
    public class CleanupPublishQueue : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                JobDataMap dataMap = context.MergedJobDataMap;

                var daysToKeep = dataMap.GetIntValue("DaysToKeep");

                Tasks.CleanupPublishQueue cleanPQTask = new Tasks.CleanupPublishQueue();
                cleanPQTask.DaysToKeep = daysToKeep;
                cleanPQTask.Run();


                Log.Info(String.Format("Job \"{0}\" triggered at {1} UTC with DaysToKeep configured as {2} days.", context.JobDetail.Key, context.Trigger.GetPreviousFireTimeUtc(), daysToKeep), this);
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
        }
    }
}
