using Quartz;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Scheduler
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    class SyncProductReviewsJob : IJob
    {


        public void Execute(IJobExecutionContext context)
        {
            //JobDataMap dataMap = context.JobDetail.JobDataMap;
            JobDataMap dataMap = context.MergedJobDataMap;

            string name = dataMap.GetString("name");
            string greeting = dataMap.GetString("greeting");
            
            Log.Info(String.Format("Job {0} triggered at {1}", context.JobDetail.Key, context.Trigger.GetPreviousFireTimeUtc()), this);
            Log.Info(String.Format("Synhronizing Product Reviews with parameters name: {0} !!, and greeting: {1} !!", name, greeting), this);
            Random r = new Random();
            Thread.Sleep(new TimeSpan(0,0,(r.Next(1, 50))));
        }
    }
}
