using Quartz;
using System;
using Sitecore.Diagnostics;


namespace Sitecore.QuartzScheduler.Schedules
{
    public class CleanupPublishQueue : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            //JobDataMap dataMap = context.JobDetail.JobDataMap;
            JobDataMap dataMap = context.MergedJobDataMap;

            var daysToKeep = dataMap.GetIntValue("DaysToKeep");

            Tasks.CleanupPublishQueue cleanPQTask = new Tasks.CleanupPublishQueue();
            cleanPQTask.DaysToKeep = daysToKeep;
            cleanPQTask.Run();

            
            Log.Info(String.Format("Job \"{0}\" triggered at {1} UTC with DaysToKeep configured as {2} days.", context.JobDetail.Key, context.Trigger.GetPreviousFireTimeUtc(), daysToKeep), this);
        }
    }
}
