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
            //var returnValue = new JsonResult();
            //returnValue.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            string jsonData = "{}";
            try
            {
                //string jsonData = "{\"totalRecordCount\":0, \"messages\":[{\"id\":100,\"messageType\":\"info\",\"text\":" +
                //    "\"Contact most likely to buy during next visit.\"}],\"pageNumber\":1," +
                //    "\"pageSize\":5,\"sorting\":[{\"direction\":\"asc\",  \"field\":\"channel\"},{\"direction\":\"asc\",\"field\":\"device\"" +
                //    "}],\"filter\":[],\"data\":{\"dataSet\":{\"visits\":[{\"Channel\":10,\"IsCampaign\":true,\"Device\":\"\"," +
                //    "\"Location\":\"Copenhagen, Hovedstaden, Denmark\",\"Value\":\"1024\",\"PageViews\":18,\"VisitDuration\":\"01:07:00\"," +
                //    "\"Recency\":\"2.00:00:00\",\"StartDateTime\":\"2013-11-08T22:30:29.2274325Z\"," +
                //    "\"UserAgent\":\"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko)\"}]}," +
                //    "\"localization\":{\"fields\":[{\"field\":\"channel\",\"translations\":{\"40\":\"RSS\",\"0\":\"Unknown\"," +
                //    "\"15\":\"Search Engine - Organic Branded\",\"20\":\"Direct\",\"50\":\"Email\",\"10\":\"Search Engine - Organic\"," +
                //    "\"90\":\"Paid\",\"36\":\"Referred - Analyst\",\"31\":\"Referred - Blog\",\"34\":\"Referred - Community\"," +
                //    "\"33\":\"Referred - Conversations\",\"32\":\"Referred - News\", \"30\":\"Referred - Other\",\"35\":\"Referred - Wiki\"" +
                //    "}}]}}}";
                List<TriggerStatistic> triggerStats = null;
                string triggerStatProviderType = ConfigurationManager.AppSettings.Get("Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider");

                if (!String.IsNullOrEmpty(triggerStatProviderType))
                {
                    var triggerStatsProvider = Activator.CreateInstance(Type.GetType(triggerStatProviderType)) as ITriggerStatisticsStore;
                    triggerStats = triggerStatsProvider.GetAllTriggerStatistics();
                    //triggerStats.Select(x => x.)
                    if (triggerStats == null)
                    {
                        Diagnostics.Log.Warn("No chart data retrieved from configured Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider when GetAllTriggerStatistics is called!", this);
                    }
                }
                else
                {
                    Diagnostics.Log.Warn("Missing App Setting value for Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider", this);
                }
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                jsonData = jsSerializer.Serialize(triggerStats);
                string requiredStructure = "{\"data\": {" +
                    "\"dataset\": [{" +
                    "\"data\": ";
                jsonData = jsonData.Insert(0, requiredStructure);
                jsonData = jsonData.Insert(jsonData.Length, "}]}}");

                //Diagnostics.Log.Info(String.Format("Here is the job performance data being returned from the controller : {0}", jsonData), this);
                //returnValue.Data = jsonData;
            }
            catch(Exception ex)
            {
                Diagnostics.Log.Error(ex.Message, this);
                throw ex;
            }
            return (FileResult)this.File(new UTF8Encoding().GetBytes(jsonData), "text/json", "data.json"); ;
        }
    }
}