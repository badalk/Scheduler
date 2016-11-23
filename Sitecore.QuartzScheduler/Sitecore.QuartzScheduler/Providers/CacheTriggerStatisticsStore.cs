using Sitecore.QuartzScheduler.Models;
using Sitecore.QuartzScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.IO;
using Sitecore.QuartzScheduler.Common;
using System.Text;
using System.Reflection;

namespace Sitecore.QuartzScheduler.Providers
{
    public class CacheTriggerStatisticsStore : ITriggerStatisticsStore
    {

        ObjectCache cache = MemoryCache.Default;


        public void SaveTriggerStatistic(TriggerStatistic triggerStat)
        {
            //Cache cacheItem = new System.Web.Caching.Cache();
            List<TriggerStatistic> triggerStats = (List<TriggerStatistic>)cache[Common.Constants.PerformanceDataCacheKey];

            //If JobStat collection is null or empty, initialize it first before adding a new entry
            if (triggerStats == null)
            {
                triggerStats = new List<TriggerStatistic>();
            }

            triggerStats.Add(triggerStat);
            cache[Common.Constants.PerformanceDataCacheKey] = triggerStats;
        }

        public List<TriggerStatistic> GetAllTriggerStatistics()
        {
            List<TriggerStatistic> triggerStats = (List<TriggerStatistic>)cache[Common.Constants.PerformanceDataCacheKey];
            return triggerStats;
        }

        public List<TriggerStatistic> GetTriggerStatisticsForGroup(string groupName)
        {
            var triggerStatsForGroup = from stats in (List<TriggerStatistic>)cache[Common.Constants.PerformanceDataCacheKey]
                                       where stats.Group.Equals(groupName, StringComparison.InvariantCultureIgnoreCase)
                                       select stats;

            return (List<TriggerStatistic>)triggerStatsForGroup;
        }
        public List<TriggerStatSummary> GetTriggerStatisticsSummary()
        {
            var triggerStats = (List<TriggerStatistic>)cache[Common.Constants.PerformanceDataCacheKey];
            List<TriggerStatSummary> summaryStat = new List<TriggerStatSummary>();

            var triggerAvgDurationList = (from ts in triggerStats
                                          group ts by ts.JobKey into grp
                                          select new
                                          {
                                              JobKey = grp.Key,
                                              DurationType = "Avg",
                                              Duration = grp.Average(p => p.ExecutionDurationInSeconds)
                                          }).ToList();
            foreach (var tss in triggerAvgDurationList)
            {
                var trigStatSummary = new TriggerStatSummary()
                {
                    JobKey = tss.JobKey,
                    DurationType = tss.DurationType,
                    Duration = tss.Duration
                };

                summaryStat.Add(trigStatSummary);
            }

            var triggerMaxDurationList = (from ts in triggerStats
                                          group ts by ts.JobKey into grp
                                          select new
                                          {
                                              JobKey = grp.Key,
                                              DurationType = "Max",
                                              Duration = grp.Max(p => p.ExecutionDurationInSeconds)
                                          }).ToList();

            foreach (var tss in triggerMaxDurationList)
            {
                var trigStatSummary = new TriggerStatSummary()
                {
                    JobKey = tss.JobKey,
                    DurationType = tss.DurationType,
                    Duration = tss.Duration
                };

                summaryStat.Add(trigStatSummary);
            }
            return summaryStat;
        }

        public List<TriggerStatistic> GetTriggerStatisticsForJob(string jobKey)
        {
            var triggerStats = (List<TriggerStatistic>)cache[Common.Constants.PerformanceDataCacheKey];
            var triggerStatsForGroup = from stats in triggerStats
                                       where stats.JobKey.Equals(jobKey, StringComparison.InvariantCultureIgnoreCase)
                                       select stats;

            return triggerStatsForGroup.ToList<TriggerStatistic>();
        }
        private string AssemblyDirectory()
        {

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);

        }
        public void ArchiveTriggerStatistics(int DaysToKeep, string ArchiveLocation)
        {
            try
            {
                string directory = AssemblyDirectory();
                DirectoryInfo dirInfo = new DirectoryInfo(directory);
                var parentPath = dirInfo.Parent;

                string archivePath = Path.Combine(parentPath.FullName, ArchiveLocation);
                if (!Directory.Exists(archivePath))
                    Directory.CreateDirectory(archivePath);

                var cacheData = cache[Common.Constants.PerformanceDataCacheKey];
                if (cacheData != null)
                {
                    var triggerStatsToArchive = from stats in (List<TriggerStatistic>) cacheData
                                                where stats.StartTime < DateTime.Now.Add(TimeSpan.FromDays(-1 * DaysToKeep))
                                                select stats;

                    var triggerStatsToKeep = from stats in (List<TriggerStatistic>) cacheData
                                             where stats.StartTime >= DateTime.Now.Add(TimeSpan.FromDays(-1 * DaysToKeep))
                                             select stats;

                    string jsonDataToArchive = HelperUtility.GetJsonSerializedData(triggerStatsToArchive.ToList<TriggerStatistic>());
                    jsonDataToArchive = HelperUtility.AddJsonHeader(jsonDataToArchive);

                    string fileName = "JobPerformanceData-" + DateTime.Now.Ticks.ToString() + ".json";
                    string filePath = Path.Combine(archivePath, fileName);

                    if (triggerStatsToArchive != null && triggerStatsToArchive.Count() > 0)
                    {
                        using (FileStream fs = File.Create(filePath))
                        {
                            byte[] data = new UTF8Encoding().GetBytes(jsonDataToArchive);
                            fs.Write(data, 0, data.Length);
                            cache[Common.Constants.PerformanceDataCacheKey] = triggerStatsToKeep.ToList<TriggerStatistic>();

                            Diagnostics.Log.Info(String.Format("Performance Data Archived at {0} location in file {1}", archivePath, fileName), this);
                        }
                    }
                    else
                    {
                        Diagnostics.Log.Info(String.Format("No Job performance data to Archive at {0}", DateTime.Now), this);
                    }
                }
            }
            catch(Exception ex)
            {
                Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
        }

        public List<TriggerStatistic> GetTriggerStatisticsForJobTrigger(string jobKey, string triggerKey)
        {
            var triggerStatsForGroup = from stats in (List<TriggerStatistic>)cache[Common.Constants.PerformanceDataCacheKey]
                                   where stats.JobKey.Equals(jobKey, StringComparison.InvariantCultureIgnoreCase)
                                   where stats.TriggerKey.Equals(triggerKey, StringComparison.InvariantCultureIgnoreCase)
                                   select stats;

            return (List<TriggerStatistic>) triggerStatsForGroup;
        }

    }
}