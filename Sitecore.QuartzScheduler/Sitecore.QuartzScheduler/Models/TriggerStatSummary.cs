using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.QuartzScheduler.Models
{
    [Serializable]
    public class TriggerStatSummary
    {
        public string JobKey { get; set; }

        public string DurationType { get; set; }

        public double Duration { get; set; }
    }
}