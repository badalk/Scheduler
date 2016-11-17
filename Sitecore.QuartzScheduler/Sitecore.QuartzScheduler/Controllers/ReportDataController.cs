using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sitecore.QuartzScheduler.Common;
using Sitecore.QuartzScheduler.Models;
using Sitecore.QuartzScheduler.Providers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Sitecore.QuartzScheduler.Controllers
{
    public class ReportDataController : Controller
    {
        // GET: ReportData
        public FileResult GetJobPerformanceData()
        {
            string jsonData = "{}";
            try
            {
                List<TriggerStatistic> triggerStats = null;
                var triggerStatStore = GetConfiguredTriggerStatStore();

                if (triggerStatStore != null)
                {
                    triggerStats = triggerStatStore.GetAllTriggerStatistics();
                    if (triggerStats != null)
                    {
                        jsonData = HelperUtility.GetJsonSerializedData(triggerStats);
                        jsonData = HelperUtility.AddJsonHeader(jsonData);
                    }
                    else
                    {
                        Diagnostics.Log.Warn("No chart data retrieved from configured Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider when GetAllTriggerStatistics is called!", this);
                    }
                }

                Diagnostics.Log.Info(String.Format("Job performance data being returned from the controller action ReportDataController.GetJobPerformanceData: {0}", jsonData), this);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
            return (FileResult)this.File(new UTF8Encoding().GetBytes(jsonData), "text/json", "data.json"); ;
        }

        public FileResult GetJobWisePerformanceData()
        {
            string jsonData = "";
            string finalJsonReportData = "";

            try
            {
                List<TriggerStatistic> triggerStats = null;

                var triggerStatStore = GetConfiguredTriggerStatStore();

                if (triggerStatStore != null)
                {
                    JobManager jm = new JobManager();
                    var jobList = jm.GetAllJobs();
                    if (jobList != null && jobList.Count > 0)
                    {
                        for (int i = 0; i < jobList.Count; i++)
                        {
                            JobDetail jd = jobList[i];

                            triggerStats = triggerStatStore.GetTriggerStatisticsForJob(jd.JobKey);
                            if (triggerStats != null)
                            {
                                jsonData = HelperUtility.GetJsonSerializedData(triggerStats);
                                if (i < jobList.Count - 1)
                                    jsonData = jsonData.Insert(jsonData.Length, ", ");

                                finalJsonReportData = finalJsonReportData.Insert(finalJsonReportData.Length, jsonData);
                            }
                            else
                            {
                                Diagnostics.Log.Warn("No chart data retrieved from configured Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider when GetAllTriggerStatistics is called!", this);
                            }
                        }
                        finalJsonReportData = HelperUtility.AddJsonHeader(finalJsonReportData);
                    }
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
            return (FileResult)this.File(new UTF8Encoding().GetBytes(finalJsonReportData), "text/json", "data.json"); ;
        }

        public FileResult GetJobPerformanceSummary()
        {
            string jsonData = "{}";
            try
            {
                List<TriggerStatSummary> triggerStatsSummary = null;
                var triggerStatStore = GetConfiguredTriggerStatStore();

                if (triggerStatStore != null)
                {
                    triggerStatsSummary = triggerStatStore.GetTriggerStatisticsSummary();

                    if (triggerStatsSummary != null)
                    {
                        jsonData = HelperUtility.GetJsonSerializedData(triggerStatsSummary);
                        jsonData = HelperUtility.AddJsonHeader(jsonData);
                    }
                    else
                    {
                        Diagnostics.Log.Warn("No chart data retrieved from configured Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider when GetJobPerformanceSummary is called!", this);
                    }
                }

                Diagnostics.Log.Info(String.Format("Job performance Summary being returned from the controller action ReportDataController.GetJobPerformanceSummary: {0}", jsonData), this);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
            return (FileResult)this.File(new UTF8Encoding().GetBytes(jsonData), "text/json", "data.json"); ;
        }

        private ITriggerStatisticsStore GetConfiguredTriggerStatStore()
        {
            string triggerStatProviderType = ConfigurationManager.AppSettings.Get("Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider");
            ITriggerStatisticsStore triggerStatsStore = null;

            if (!String.IsNullOrEmpty(triggerStatProviderType))
            {
                triggerStatsStore = Activator.CreateInstance(Type.GetType(triggerStatProviderType)) as ITriggerStatisticsStore;
            }
            else
            {
                Diagnostics.Log.Warn("Missing App Setting value for Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider", this);
            }
            return triggerStatsStore;

        }
    }
}