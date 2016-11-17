using Sitecore.QuartzScheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.QuartzScheduler.Providers
{
    interface ITriggerStatisticsStore
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobStat"></param>
        void SaveTriggerStatistic(TriggerStatistic jobStat);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<TriggerStatistic> GetAllTriggerStatistics();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        List<TriggerStatistic> GetTriggerStatisticsForGroup(string groupName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        List<TriggerStatistic> GetTriggerStatisticsForJob(string jobKey);


        List<TriggerStatSummary> GetTriggerStatisticsSummary();

        void ArchiveTriggerStatistics(int DaysToKeep, string ArchiveLocation);
    }
}
