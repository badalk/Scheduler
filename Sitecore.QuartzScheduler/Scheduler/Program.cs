using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler
{
    //class Program
    //{
    //    static ITriggerListener trigListener;
    //    static ISchedulerFactory schedFact;
    //    static IScheduler sched;
    //    static void Main(string[] args)
    //    {
    //        JobManager jm = new JobManager();
    //        jm.ScheduleJobs();

    //        //jm.GetAllJobs();

    //    }



    //    //private static void ScheduleJobs()
    //    //{
    //    //    try
    //    //    {

    //    //        // get a scheduler
    //    //        sched.Start();

    //    //        // define the job and tie it to our HelloJob class
    //    //        IJobDetail job = JobBuilder.Create(typeof(HelloJob))
    //    //            .WithIdentity("myJob", "group1")
    //    //            .UsingJobData("path", @"D:\Test")
    //    //            .UsingJobData("tablestoclean", "Log, ContentStatistics")
    //    //            .UsingJobData("value", 2)
    //    //            .Build();

    //    //        // Trigger the job to run now, and then every 40 seconds
    //    //        ITrigger trigger = TriggerBuilder.Create()
    //    //          .WithIdentity("myTrigger", "group1")
    //    //          .StartNow()
    //    //          .WithSimpleSchedule(x => x
    //    //              .WithIntervalInSeconds(10)
    //    //              .WithRepeatCount(5))
    //    //          .Build();

    //    //        //Add Job and Trigger listeners
    //    //        sched.ListenerManager.AddTriggerListener(trigListener, EverythingMatcher<JobKey>.AllTriggers());


    //    //        sched.ScheduleJob(job, trigger);


    //    //        //CronScheduleBuilder csb = CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(10, 10, DayOfWeek.Monday, DayOfWeek.Tuesday);
    //    //        //    csb = CronScheduleBuilder.DailyAtHourAndMinute(10, 10);
    //    //        //csb = CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(5, 10, 0);
    //    //        //csb = CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Sunday, 10,10 );
    //    //        //csb = CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Sunday, 10,10 );

                
    //    //    }
    //    //    catch (JobExecutionException jobExeEX)
    //    //    {
    //    //        Console.WriteLine(jobExeEX.Message + Environment.NewLine + jobExeEX.StackTrace);
    //    //    }
    //    //}
    //}
}
