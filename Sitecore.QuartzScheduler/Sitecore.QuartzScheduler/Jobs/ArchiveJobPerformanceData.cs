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
using System.Configuration;
using Sitecore.QuartzScheduler.Providers;
using Sitecore.Configuration;

namespace Sitecore.QuartzScheduler.Jobs
{
    [DisallowConcurrentExecution]

    public class ArchiveJobPerformanceData: IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                JobDataMap dataMap = context.MergedJobDataMap;

                int daysToKeep = dataMap.GetIntValue("DaysToKeep");
                string archivePath = dataMap.GetString("Archive_Path");

                string triggerStatProviderType = Settings.GetSetting("Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider");
                ITriggerStatisticsStore triggerStatsStore = null;

                if (!String.IsNullOrEmpty(triggerStatProviderType))
                {
                    triggerStatsStore = Activator.CreateInstance(Type.GetType(triggerStatProviderType)) as ITriggerStatisticsStore;
                    triggerStatsStore.ArchiveTriggerStatistics(daysToKeep, archivePath);
                }
                else
                {
                    Diagnostics.Log.Warn("Missing App Setting value for Sitecore.QuartzScheduler.TriggerStatisticsStoreProvider", this);
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }

}
    }
}
