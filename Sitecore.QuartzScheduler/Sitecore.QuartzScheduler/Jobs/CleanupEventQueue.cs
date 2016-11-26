using Quartz;
using System;
using Sitecore.Diagnostics;


namespace Sitecore.QuartzScheduler.Jobs
{
    [DisallowConcurrentExecution]
    public class CleanupEventQueue : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                JobDataMap dataMap = context.MergedJobDataMap;

                var daysToKeep = dataMap.GetIntValue("DaysToKeep");
                var intervalToKeep = dataMap.GetString("IntervalToKeep (hh:mm:ss)");

                Tasks.CleanupEventQueue cleanEventQ = new Tasks.CleanupEventQueue();
                cleanEventQ.DaysToKeep = (uint) daysToKeep;
                cleanEventQ.IntervalToKeep = intervalToKeep;
                cleanEventQ.Run();


                Log.Info(String.Format("Job \"{0}\" triggered at {1} UTC with DaysToKeep configured as {2} days.", context.JobDetail.Key, context.Trigger.GetPreviousFireTimeUtc(), daysToKeep), this);
            }
            catch (Exception ex)
            {
                Log.Error("Sitecore.QuartzScheuler: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
        }
    }
}
