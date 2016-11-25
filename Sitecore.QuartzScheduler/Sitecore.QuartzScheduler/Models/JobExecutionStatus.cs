using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.QuartzScheduler.Models
{
    [Serializable]
    public class JobExecutionStatus
    {
        public string JobKey { get; set; }

        public DateTime? FireTime { get; set; }
        public DateTime? PreviousFireTime { get; set; }
        public DateTime? ScheduledFireTime { get; set; }
        public DateTime? NextFireTime { get; set; }
        public double JobRunTime { get; set; }
        public string TriggerName { get; set; }
        public string State { get; set; }
    }
}