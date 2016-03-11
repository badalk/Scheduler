using Quartz;
using Quartz.Listener;
using System;
using System.Diagnostics;
using System.Configuration;
using Sitecore.QuartzScheduler.Providers;
using Sitecore.QuartzScheduler.Models;

namespace Sitecore.QuartzScheduler.Listeners
{
    public class SchedulerTriggerListener : TriggerListenerSupport 
    {
        Stopwatch sw;

        public override string Name
        {
            get { return this.GetType().ToString(); }
        }

        public override void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            try
            {
                sw.Stop();
                TriggerStatistic triggerStat = new TriggerStatistic()
                {
                    Group = trigger.Key.Group,
                    JobKey = trigger.JobKey.Name,
                    TriggerKey = trigger.Key.Name,
                    ExecutionDurationInseconds = sw.Elapsed.TotalSeconds,
                    StartTime = trigger.StartTimeUtc.DateTime.ToLocalTime(),
                    FinishTime = DateTime.Now,
                };


                Sitecore.Diagnostics.Log.Info(String.Format("Job {0} with trigger {1} Completed @ {2} and it took {3} seconds ", triggerStat.JobKey, triggerStat.TriggerKey, DateTime.Now, triggerStat.ExecutionDurationInseconds), this);
                
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

        public override void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            try
            {
                sw = Stopwatch.StartNew();
                Sitecore.Diagnostics.Log.Info(String.Format("Job {0} Started @ {1}", context.JobDetail.Key, DateTime.Now.ToLocalTime()), this);
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Info("Exception in TriggerFired: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
            }
        }

    }
}
