using Sitecore.QuartzScheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace Sitecore.QuartzScheduler.Providers
{
    public class CacheTriggerStatisticsStore : ITriggerStatisticsStore
    {

        ObjectCache cache = MemoryCache.Default;


        public void SaveTriggerStatistic(TriggerStatistic triggerStat)
        {
            //Cache cacheItem = new System.Web.Caching.Cache();
            List<TriggerStatistic> triggerStats = (List<TriggerStatistic>)cache["TriggerStats"];

            //If JobStat collection is null or empty, initialize it first before adding a new entry
            if (triggerStats == null)
            {
                triggerStats = new List<TriggerStatistic>();
            }

            triggerStats.Add(triggerStat);
            cache["TriggerStats"] = triggerStats;
        }

        public List<TriggerStatistic> GetAllTriggerStatistics()
        {
            List<TriggerStatistic> triggerStats = (List<TriggerStatistic>)cache["TriggerStats"];
            return triggerStats;
        }

        public List<TriggerStatistic> GetTriggerStatisticsForGroup(string groupName)
        {
            var triggerStatsForGroup = from stats in (List<TriggerStatistic>)cache["TriggerStats"]
                                   where stats.Group.Equals(groupName, StringComparison.InvariantCultureIgnoreCase)
                                   select stats;

            return  (List<TriggerStatistic>) triggerStatsForGroup;
        }
        //public List<TriggerStatistic> GetTriggerStatisticsSummary()
        //{
        //    var triggerStats = (List<TriggerStatistic>)cache["TriggerStats"];

        //    var triggerStatsForGroup = from stats in triggerStats
        //                               //where stats.JobKey.Equals(jobKey, StringComparison.InvariantCultureIgnoreCase)
        //                               select { JobKey = stats.JobKey, ExecutionDurationInSeconds = stats.ExecutionDurationInSeconds};
        //}

        public List<TriggerStatistic> GetTriggerStatisticsForJob(string jobKey)
        {
            var triggerStats = (List<TriggerStatistic>)cache["TriggerStats"];
            var triggerStatsForGroup = from stats in triggerStats
                                   where stats.JobKey.Equals(jobKey, StringComparison.InvariantCultureIgnoreCase)
                                   select stats;

            return triggerStatsForGroup.ToList<TriggerStatistic>();
        }

        public List<TriggerStatistic> GetTriggerStatisticsForJobTrigger(string jobKey, string triggerKey)
        {
            var triggerStatsForGroup = from stats in (List<TriggerStatistic>)cache["TriggerStats"]
                                   where stats.JobKey.Equals(jobKey, StringComparison.InvariantCultureIgnoreCase)
                                   where stats.TriggerKey.Equals(triggerKey, StringComparison.InvariantCultureIgnoreCase)
                                   select stats;

            return (List<TriggerStatistic>) triggerStatsForGroup;
        }

    }
}