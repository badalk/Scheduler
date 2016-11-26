using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
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

                Diagnostics.Log.Info(String.Format("Sitecore.QuartzScheuler: Job performance data being returned from the controller action ReportDataController.GetJobPerformanceData: {0}", jsonData), this);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Sitecore.QuartzScheuler: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
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
                            if (triggerStats != null && triggerStats.Count > 0)
                            {
                                jsonData = HelperUtility.GetJsonSerializedData(triggerStats);
                                if (i < jobList.Count - 1)
                                    jsonData = jsonData.Insert(jsonData.Length, ", ");

                                finalJsonReportData = finalJsonReportData.Insert(finalJsonReportData.Length, jsonData);
                            }
                            else
                            {
                                Diagnostics.Log.Warn("No chart data retrieved from configured Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider when GetTriggerStatisticsForJob is called!", this);
                            }
                        }

                        finalJsonReportData = HelperUtility.AddJsonHeader(finalJsonReportData);
                    }
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Sitecore.QuartzScheuler: Error in GetJobWisePerformanceData : " + ex.Message + Environment.NewLine + ex.StackTrace, this);
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
                Diagnostics.Log.Error("Sitecore.QuartzScheuler: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
            return (FileResult)this.File(new UTF8Encoding().GetBytes(jsonData), "text/json", "data.json"); ;
        }

        private ITriggerStatisticsStore GetConfiguredTriggerStatStore()
        {
            string triggerStatProviderType = Settings.GetSetting("Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider");
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

        public ActionResult GetJobDefinitions()
        {

            var returnValue = new JsonResult();
            returnValue.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            List<JobDetail> jobList = new List<JobDetail>();
            JobManager jm = new JobManager();

            try
            {

                jobList = jm.GetConfiguredJobs();

                returnValue.Data = HelperUtility.GetJsonSerializedData(jobList, false);
            }
            catch(Exception ex)
            {
                Log.Error("Sitecore.QuartzScheuler: Error Occured in JobManager.GetConfiguredJobs : " + ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }

            return returnValue;
        }

        public ActionResult GetJobTriggerList(string jobId)
        {
            var returnValue = new JsonResult();
            returnValue.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            JobManager jm = new JobManager();
            var jobDetail = jm.GetJobDetails(jobId);
            var triggerList = jm.GetTriggersForJob(jobDetail);
            returnValue.Data = HelperUtility.GetJsonSerializedData(triggerList, false); 

            return returnValue;
        }

        public ActionResult GetCurrentJobExecutionStatus()
        {
            var returnValue = new JsonResult();
            returnValue.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                JobManager jm = new JobManager();
                var currentJobList = jm.GetCurrentJobStatus();
                returnValue.Data = HelperUtility.GetJsonSerializedData(currentJobList, false);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Sitecore.QuartzScheuler: Error in GetCurrentJobExecutionStatus: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
            return returnValue;

        }

        public ActionResult GetJobFireTimes()
        {
            var returnValue = new JsonResult();
            returnValue.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                JobManager jm = new JobManager();
                var currentJobList = jm.GetJobFireTimes();
                returnValue.Data = HelperUtility.GetJsonSerializedData(currentJobList, false);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Sitecore.QuartzScheuler: Error in GetJobFireTimes : " + ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
            return returnValue;

        }
    }
}