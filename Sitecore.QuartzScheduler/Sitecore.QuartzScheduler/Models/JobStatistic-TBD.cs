using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Sitecore.QuartzScheduler.Models
{
    [Serializable]
    public class JobStatistic
    {
        public string Group { get; set; }
        public string JobKey { get; set; }
        public string TriggerKey { get; set; }

        public double AvgExecutionTime
        {
            get
            {
                ObjectCache cache = MemoryCache.Default;
                var triggerStats = cache["TriggerStats"] as List<TriggerStatistic>;
                if (triggerStats != null)
                {
                    var averageDuration = from p in triggerStats
                                          where (p.Group.Equals(this.Group) && p.JobKey.Equals(this.JobKey))
                                          group p by new { p.Group, p.JobKey } into g
                                          select new { avgTime = g.Average(p => p.ExecutionDurationInseconds) };

                    return averageDuration.FirstOrDefault().avgTime;


                }
                else
                {
                    return 0;
                }

            }
        }

        public double MaximumExecutionTime
        {
            get
            {
                ObjectCache cache = MemoryCache.Default;
                var triggerStats = cache["TriggerStats"] as List<TriggerStatistic>;
                if (triggerStats != null)
                {
                    var averageDuration = from p in triggerStats
                                          where (p.Group.Equals(this.Group) && p.JobKey.Equals(this.JobKey))
                                          group p by new { p.Group, p.JobKey } into g
                                          select new { maxTime = g.Max(p => p.ExecutionDurationInseconds) };

                    return averageDuration.FirstOrDefault().maxTime;


                }
                else
                {
                    return 0;
                }

            }

        }
    }
}
