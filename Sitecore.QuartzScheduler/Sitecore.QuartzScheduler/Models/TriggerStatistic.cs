using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.QuartzScheduler.Models
{
    [Serializable]
    public class TriggerStatistic
    {
        public string Group { get; set; }
        public string JobKey { get; set; }
        public string TriggerKey { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }


        //TODO: Need to un-comment these properties
        //public DateTime? PreviousFireTime
        //{
        //    get
        //    {
        //        JobManager jobMgr = new JobManager();
        //        return jobMgr.GetPreviousFireTime(Group, TriggerKey);
        //    }
        //}

        //public DateTime NextFireTime {
        //    get
        //    {
        //        JobManager jobMgr = new JobManager();
        //        return jobMgr.GetNextFireTime(Group, TriggerKey);
        //    }
        //}

        //public string State
        //{
        //    get
        //    {
        //        JobManager jobMgr = new JobManager();
        //        return jobMgr.GetTriggerState(TriggerKey, Group);
        //    }
        //}

        public double ExecutionDurationInSeconds { get; set; }

    }
}
