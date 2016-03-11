using Sitecore.QuartzScheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.QuartzScheduler
{
    public static class JobManagerUtility
    {
        public static string GetJobDataMapSitecoreString(Quartz.JobDataMap jobDataMap)
        {
            StringBuilder sbJobDataMap = new StringBuilder();
            //Convert Quartz Job Data Map to .net string having each name value pair delimted & as represented by NameValueField in sitecore
            if (jobDataMap != null && jobDataMap.Count > 0)
            {
                IList<string> jobDataMapKeys = jobDataMap.GetKeys();
                for (int i = 0; i < jobDataMapKeys.Count; i++)
                {
                    sbJobDataMap.Append(jobDataMapKeys[i] + "=" + jobDataMap.GetString(jobDataMapKeys[i]));
                    if (i < jobDataMapKeys.Count - 1)
                    {
                        sbJobDataMap.Append("&");
                    }
                }
            }
            return sbJobDataMap.ToString();

        }
    }
}