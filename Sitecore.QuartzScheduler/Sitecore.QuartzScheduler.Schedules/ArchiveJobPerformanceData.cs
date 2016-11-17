using Quartz;
using System;
using System.Web;
//using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Runtime.Caching;
using System.Reflection;

namespace Sitecore.QuartzScheduler.Schedules
{
    public class ArchiveJobPerformanceData: IJob
    {
        public void Execute(IJobExecutionContext context)
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


        }
    }
}
