using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.QuartzScheduler.Listeners
{
    public class SchedulerListener : ISchedulerListener
    {
        public void JobAdded(IJobDetail jobDetail)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.JobAdded \"{0}\" at {1}", jobDetail.Key.Name, DateTime.Now), this);
        }

        public void JobDeleted(JobKey jobKey)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.JobDeleted \"{0}\" at {1}", jobKey.Name, DateTime.Now), this);
        }

        public void JobPaused(JobKey jobKey)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.JobPaused \"{0}\" at {1}", jobKey.Name, DateTime.Now), this);
        }

        public void JobResumed(JobKey jobKey)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.Resumed \"{0}\" at {1}", jobKey.Name, DateTime.Now), this);
        }

        public void JobScheduled(ITrigger trigger)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.JobScheduled \"{0}\" with trigger {1} at {2}", trigger.JobKey.Name, trigger.Key.Name, DateTime.Now), this);
        }

        public void JobsPaused(string jobGroup)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.JobsPaused for a group \"{0}\" at {1}", jobGroup, DateTime.Now), this);
        }

        public void JobsResumed(string jobGroup)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.JobsResumed for a group \"{0}\" at {1}", jobGroup, DateTime.Now), this);
        }

        public void JobUnscheduled(TriggerKey triggerKey)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.JobUnScheduled with trigger {0} at {1}", triggerKey.ToString(), DateTime.Now), this);
        }

        public void SchedulerError(string msg, SchedulerException cause)
        {
            Sitecore.Diagnostics.Log.Error(String.Format("Sitecore.QuartzScheuler: SchedulerListener.SchedulerError with Message: \"{0}\" with exception: {1} at {2}", 
                                            msg, cause.Message + Environment.NewLine + cause.StackTrace, DateTime.Now), this);
        }

        public void SchedulerInStandbyMode()
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.SchedulerInStandbyMode at {0}", DateTime.Now), this);
        }

        public void SchedulerShutdown()
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.SchedulerShutdown at {0}", DateTime.Now), this);
        }

        public void SchedulerShuttingdown()
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.SchedulerShuttingdown at {0}", DateTime.Now), this);
        }

        public void SchedulerStarted()
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.SchedulerStarted at {0}", DateTime.Now), this);
        }

        public void SchedulerStarting()
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.SchedulerStarting at {0}", DateTime.Now), this);
        }

        public void SchedulingDataCleared()
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.SchedulingDataCleared at {0}", DateTime.Now), this);
        }

        public void TriggerFinalized(ITrigger trigger)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.TriggerFinalized for trigger {0} at {1}", trigger.Key.ToString(), DateTime.Now), this);
        }

        public void TriggerPaused(TriggerKey triggerKey)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.TriggerPaused for trigger {0} at {1}", triggerKey.ToString(), DateTime.Now), this);
        }

        public void TriggerResumed(TriggerKey triggerKey)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.TriggerResumed for trigger {0} at {1}", triggerKey.ToString(), DateTime.Now), this);
        }

        public void TriggersPaused(string triggerGroup)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.TriggersPaused for a group {0} at {1}", triggerGroup, DateTime.Now), this);
        }

        public void TriggersResumed(string triggerGroup)
        {
            Sitecore.Diagnostics.Log.Info(String.Format("SchedulerListener.TriggersResumed for a group {0} at {1}", triggerGroup, DateTime.Now), this);
        }
    }
}