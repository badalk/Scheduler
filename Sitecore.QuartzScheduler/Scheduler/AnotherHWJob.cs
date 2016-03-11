using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Scheduler
{
    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    class AnotherHWJob : IJob
    {


        public void Execute(IJobExecutionContext context)
        {
            //JobDataMap dataMap = context.JobDetail.JobDataMap;
            JobDataMap dataMap = context.MergedJobDataMap;

            string name = dataMap.GetString("name");
            string greeting = dataMap.GetString("greeting");

            Console.WriteLine(String.Format("Hello {1} !!, {2} !!", name, greeting));
        }
    }
}
