using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
                string triggerStatProviderType = ConfigurationManager.AppSettings.Get("Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider");

                if (!String.IsNullOrEmpty(triggerStatProviderType)) {
                    var triggerStatsProvider = Activator.CreateInstance(Type.GetType(triggerStatProviderType)) as ITriggerStatisticsStore;
                    triggerStats = triggerStatsProvider.GetAllTriggerStatistics();
                    if (triggerStats != null) {
                        jsonData = GetJsonSerializedData(triggerStats);
                        jsonData = AddJsonHeader(jsonData);
                    }
                    else {
                        Diagnostics.Log.Warn("No chart data retrieved from configured Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider when GetAllTriggerStatistics is called!", this);
                    }
                }
                else {
                    Diagnostics.Log.Warn("Missing App Setting value for Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider", this);
                }
                Diagnostics.Log.Info(String.Format("Here is the job performance data being returned from the controller action ReportDataController.GetJobPerformanceData: {0}", jsonData), this);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(ex.Message, this);
                throw ex;
            }
            return (FileResult)this.File(new UTF8Encoding().GetBytes(jsonData), "text/json", "data.json"); ;
        }

        private static string GetJsonSerializedData(List<TriggerStatistic> triggerStats)
        {
            string jsonData = JsonConvert.SerializeObject(triggerStats, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            string requiredStructure = "{" +
                                    "\"data\": ";
            jsonData = jsonData.Insert(0, requiredStructure);
            jsonData = jsonData.Insert(jsonData.Length, "}");

            return jsonData;
        }

        private static string AddJsonHeader(string jsonData)
        {
            if (!String.IsNullOrEmpty(jsonData))
            {
                string requiredStructure = "{\"data\": {" +
                                            "\"dataset\": [";
                jsonData = jsonData.Insert(0, requiredStructure);
                jsonData = jsonData.Insert(jsonData.Length, "]}}");
            }
            return jsonData;
        }

        public FileResult GetJobWisePerformanceData()
        {
            string jsonData = "";
            string finalJsonReportData = "";

            try
            {
                List<TriggerStatistic> triggerStats = null;
                string triggerStatProviderType = ConfigurationManager.AppSettings.Get("Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider");

                if (!String.IsNullOrEmpty(triggerStatProviderType)) {
                    var triggerStatsProvider = Activator.CreateInstance(Type.GetType(triggerStatProviderType)) as ITriggerStatisticsStore;
                    JobManager jm = new JobManager();
                    var jobList = jm.GetAllJobs();
                    if (jobList != null && jobList.Count > 0) {
                        for (int i = 0; i < jobList.Count; i++) {
                            JobDetail jd = jobList[i];

                            triggerStats = triggerStatsProvider.GetTriggerStatisticsForJob(jd.JobKey);
                            if (triggerStats != null) {
                                jsonData = GetJsonSerializedData(triggerStats);
                                if (i < jobList.Count - 1)
                                    jsonData = jsonData.Insert(jsonData.Length, ", ");

                                finalJsonReportData = finalJsonReportData.Insert(finalJsonReportData.Length, jsonData);
                            }
                            else {
                                Diagnostics.Log.Warn("No chart data retrieved from configured Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider when GetAllTriggerStatistics is called!", this);
                            }
                        }
                        finalJsonReportData = AddJsonHeader(finalJsonReportData);
                    }
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(ex.Message, this);
                throw ex;
            }
            return (FileResult)this.File(new UTF8Encoding().GetBytes(finalJsonReportData), "text/json", "data.json"); ;
        }
    }
}