using Quartz;
using Quartz.Listener;
using System;
using System.Diagnostics;
using System.Configuration;
using Sitecore.QuartzScheduler.Providers;
using Sitecore.QuartzScheduler.Models;

namespace Sitecore.QuartzScheduler.Listeners
{
    public class SchedulerTriggerListener : ITriggerListener 
    {
        public string Name
        {
            get { return this.GetType().ToString(); }
        }

        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            try
            {
                var executationDuration = (DateTime.UtcNow - trigger.GetPreviousFireTimeUtc()).Value.TotalSeconds;
                TriggerStatistic triggerStat = new TriggerStatistic()
                {
                    Group = trigger.Key.Group,
                    JobKey = trigger.JobKey.Name,
                    TriggerKey = trigger.Key.Name,
                    ExecutionDurationInSeconds = executationDuration,
                    StartTime = trigger.GetPreviousFireTimeUtc().Value.DateTime.ToLocalTime(),
                    FinishTime = DateTime.Now
                };


                Sitecore.Diagnostics.Log.Info(String.Format("Job {0} with trigger {1} Completed @ {2} and it took {3} seconds ", triggerStat.JobKey, triggerStat.TriggerKey, DateTime.Now, triggerStat.ExecutionDurationInSeconds), this);
                
                string triggerStatProviderType = ConfigurationManager.AppSettings.Get("Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider");

                if (!String.IsNullOrEmpty(triggerStatProviderType))
                {
                    var triggerStatsProvider = Activator.CreateInstance(Type.GetType(triggerStatProviderType)) as ITriggerStatisticsStore;
                    triggerStatsProvider.SaveTriggerStatistic(triggerStat);
                }
                else
                {
                    Sitecore.Diagnostics.Log.Warn("Missing App Setting value for Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider", this);
                }
                
            }
            catch(Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("Exception in TriggerComplete: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
            }
        }

        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            try
            {
                Sitecore.Diagnostics.Log.Info(String.Format("Job {0} Started @ {1}", context.JobDetail.Key, DateTime.Now.ToLocalTime()), this);
                Sitecore.Diagnostics.Log.Info(String.Format("Currently executing {0} Jobs", context.Scheduler.GetCurrentlyExecutingJobs().Count), this);

            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Info("Exception in TriggerFired: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
            }
        }

        public void TriggerMisfired(ITrigger trigger)
        {
            Sitecore.Diagnostics.Log.Warn(String.Format("Trigger Misfired for Job \"{0}\" at {1}", trigger.JobKey.ToString(), DateTime.Now), this);
        }

        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("In VetoJobExecution of Trigger Listener for job \"{0}\" at {1}", trigger.JobKey.ToString(), DateTime.Now), this);
            return false;
        }
    }
}
