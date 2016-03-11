using Sitecore.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.QuartzScheduler.Pipelines
{
    public class Initialize
    {
        public virtual void Process(PipelineArgs args)
        {
            JobManager jobMgr = new JobManager();
            jobMgr.ScheduleJobs();
        }
    }
}
