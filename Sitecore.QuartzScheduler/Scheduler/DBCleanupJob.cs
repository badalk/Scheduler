using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sitecore.Diagnostics;

namespace Scheduler
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class DBCleanupJob : IJob
    {

        public string path { private get; set; }
        public string tablestoclean { private get; set; }

        public void Execute(IJobExecutionContext context)
        {
            //JobDataMap dataMap = context.JobDetail.JobDataMap;
            JobDataMap dataMap = context.MergedJobDataMap;

            string path = dataMap.GetString("path");
            string tablestoclean = dataMap.GetString("tablestoclean");
            int value = dataMap.GetInt("value");

            Thread.Sleep(TimeSpan.FromSeconds(value));
            value = value + 1;

            context.JobDetail.JobDataMap["value"] = value;

            Log.Info(String.Format("Job {0} triggered at {1}", context.JobDetail.Key, context.Trigger.GetPreviousFireTimeUtc()), this);
            Log.Info(String.Format("Cleaning files from {0}, and database tables {1}, and the value is {2}", path, tablestoclean, value), this);
        }
    }
    
}
