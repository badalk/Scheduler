using Quartz;
using System;
using Sitecore.Diagnostics;


namespace Sitecore.QuartzScheduler.Jobs
{
    [DisallowConcurrentExecution]
    public class CleanupHistory : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Tasks.CleanupHistory cleanupHistory = new Tasks.CleanupHistory();
                cleanupHistory.Run();


                Log.Info(String.Format("Job \"{0}\" triggered at {1} UTC.", context.JobDetail.Key, context.Trigger.GetPreviousFireTimeUtc()), this);
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
        }
    }
}
