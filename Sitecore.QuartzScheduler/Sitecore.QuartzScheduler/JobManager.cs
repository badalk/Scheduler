using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using Sitecore.Diagnostics;
using Sitecore.QuartzScheduler.Models;
using Sitecore.Data;
using Sitecore.Configuration;
using Sitecore.QuartzScheduler.Listeners;
using Sitecore.Data.Items;
using System.Collections.Specialized;
using Sitecore.Data.Fields;
using Sitecore.QuartzScheduler.Repository;
using System.Configuration;

namespace Sitecore.QuartzScheduler
{
    public class JobManager
    {
        ITriggerListener trigListener;
        IScheduler scheduler;

        public JobManager()
        {
            //Create a SchedulerTriggerListener
            trigListener = new SchedulerTriggerListener();

            // get a scheduler
            scheduler = StdSchedulerFactory.GetDefaultScheduler();
        }

        public void ScheduleJobs()
        {
            try
            {

                //start a scheduler
                scheduler.Start();


                //Create Job Definitions and add it to the collection to simulate this is what we will get from Sitecore
                Log.Info("Getting Job Definitions", this);
                var jobDefinitions = GetConfiguredJobs();

                //Cleanup any existing jobs scheduled already 
                //to ensure that every time ScheduleJobs is called, 
                //it schedules the whole list of jobs from fresh
                Log.Info("Clearing existing scheduled Jobs before we set it up agin !", this);
                scheduler.Clear();

                foreach (JobDetail jd in jobDefinitions)
                {

                    JobDataMap jobDataMap = new JobDataMap();

                    //Get Job Data Map
                    NameValueCollection jobDataCollection = Sitecore.Web.WebUtil.ParseUrlParameters(jd.JobData);
                    foreach (string key in jobDataCollection.Keys)
                    {
                        jobDataMap.Add(key, jobDataCollection[key]);
                    }

                    // define the job and tie it to our HelloJob class
                    var job = JobBuilder.Create(Type.GetType(jd.Type, true, true))
                        .WithIdentity(jd.JobKey, jd.Group)
                        .WithDescription(jd.Description)
                        .UsingJobData(jobDataMap)
                        .Build();

                    Log.Info(String.Format("Job {0} created", jd.JobKey), this);
                    Log.Info(String.Format("Getting triggers for job {0}", jd.JobKey), this);

                    var triggersForJob = GetTriggersForJob(jd);


                    Quartz.Collection.HashSet<ITrigger> triggers = new Quartz.Collection.HashSet<ITrigger>();
                    foreach (TriggerDetail td in triggersForJob)
                    {
                        var trigger = TriggerBuilder.Create();
                        trigger.WithIdentity(td.TriggerKey, jd.Group);
                        trigger.ForJob(jd.JobKey);
                        trigger.StartAt(td.StartTime);
                        if (td.EndTime != null && !td.EndTime.Equals(DateTime.MinValue))
                            trigger.EndAt(td.EndTime);
                        //trigger.StartNow()

                        //var scheduleType = td.ScheduleType

                        switch (td.ScheduleTypeValue.ToLower())
                        {
                            case "seconds":
                                if (td.RepeatInterval > 0)
                                {
                                    if (td.RepeatCount > 0)
                                    {
                                        trigger = trigger.WithSimpleSchedule(x => x
                                                        .WithIntervalInSeconds(td.RepeatInterval)
                                                        .WithRepeatCount(td.RepeatCount)
                                                        );
                                    }
                                    else
                                    {
                                        trigger = trigger.WithSimpleSchedule(x => x
                                                        .WithIntervalInSeconds(td.RepeatInterval)
                                                        .RepeatForever()
                                                        );
                                    }
                                }
                                else
                                    Log.Warn(String.Format("Job {0} was configured with {1} schedule but no Repeat Interval mentioned. Please configure the Repeat Interval correctly !!", jd.JobKey, td.ScheduleType), this);


                                Diagnostics.Log.Info(String.Format("ScheduleType {0} and Schedule : {1})", td.ScheduleType, trigger.ToString()), this);

                                break;
                            case "minutes":
                                if (td.RepeatInterval > 0)
                                {
                                    if (td.RepeatCount > 0)
                                    {
                                        trigger = trigger.WithSimpleSchedule(x => x
                                                        .WithIntervalInMinutes(td.RepeatInterval)
                                                        .WithRepeatCount(td.RepeatCount)
                                                        );
                                    }
                                    else
                                    {
                                        trigger = trigger.WithSimpleSchedule(x => x
                                                        .WithIntervalInMinutes(td.RepeatInterval)
                                                        .RepeatForever()
                                                        );
                                    }
                                }
                                else
                                    Log.Warn(String.Format("Job {0} was configured with {1} schedule but no Repeat Interval mentioned. Please configure the Repeat Interval correctly !!", jd.JobKey, td.ScheduleType), this);


                                Diagnostics.Log.Info(String.Format("ScheduleType {0} and Schedule : {1})", td.ScheduleType, trigger.ToString()), this);

                                break;
                            case "hours":
                               if (td.RepeatInterval > 0)
                                {
                                    if (td.RepeatCount > 0)
                                    {
                                        trigger = trigger.WithSimpleSchedule(x => x
                                                        .WithIntervalInHours(td.RepeatInterval)
                                                        .WithRepeatCount(td.RepeatCount)
                                                        );
                                    }
                                    else
                                    {
                                        trigger = trigger.WithSimpleSchedule(x => x
                                                        .WithIntervalInHours(td.RepeatInterval)
                                                        .RepeatForever()
                                                        );
                                    }
                                }
                                else
                                    Log.Warn(String.Format("Job {0} was configured with {1} schedule but no Repeat Interval mentioned. Please configure the Repeat Interval correctly !!", jd.JobKey, td.ScheduleType), this);


                                Diagnostics.Log.Info(String.Format("ScheduleType {0} and Schedule : {1})", td.ScheduleType, trigger.ToString()), this);

                                break;

                            case "daily":
                                trigger.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(td.StartTime.Hour, td.StartTime.Minute));
                                break;

                            case "weekly":
                                //Convert Sitecore DaysOfWeeks property to System.DaysOfWeek which is understood by Quartz.net
                                if (td.DaysOfWeeks.Count > 0)
                                {
                                    DayOfWeek[] dayOfWeeks = new DayOfWeek[td.DaysOfWeeks.Count];
                                    for (int i = 0; i < td.DaysOfWeeks.Count; i++)
                                    {
                                        dayOfWeeks[i] = (System.DayOfWeek)Enum.Parse(typeof(System.DayOfWeek), td.DaysOfWeeks[i].ToString());
                                    }

                                    trigger.WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(td.StartTime.Hour, td.StartTime.Minute, dayOfWeeks));
                                }
                                else
                                {
                                    Log.Warn(String.Format("Job {0} was configured with {1} schedule but \"Day of Weeks\" was not set correctly. Please configure the trigger correctly !!", jd.JobKey, td.ScheduleType), this);

                                }
                                break;

                            case "monthly":
                                if (td.DayOfMonth > 0)
                                    trigger.WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(td.DayOfMonth, td.StartTime.Hour, td.StartTime.Minute));
                                else
                                    Log.Warn(String.Format("Job {0} was configured with {1} schedule but \"Day of Month\" was not set correctly. Please configure the trigger correctly !!", jd.JobKey, td.ScheduleType), this);

                                break;

                            case "custom":
                                if (!String.IsNullOrEmpty(td.CronExpression))
                                    trigger.WithSchedule(CronScheduleBuilder.CronSchedule(new CronExpression(td.CronExpression)));
                                else
                                    Log.Warn(String.Format("Job {0} was configured with {1} schedule but \"Cron Expression\" was not set correctly. Please configure the trigger correctly !!", jd.JobKey, td.ScheduleType), this);
                                break;

                        }



                        //Add Job and Trigger listeners
                        Log.Info("Registering Trigger Listener", this);
                        scheduler.ListenerManager.AddTriggerListener(trigListener, EverythingMatcher<JobKey>.AllTriggers());

                        triggers.Add(trigger.Build());
                        Log.Info(String.Format("Job {0} is scheduled with trigger {1}.", jd.JobKey, td.TriggerKey), this);

                    }

                    scheduler.ScheduleJob(job, triggers, true);
                }
            }
           
            catch (JobExecutionException jobExeEX)
            {
                Log.Error(String.Format("Error Occured in {0}", jobExeEX.Source) + jobExeEX.Message + Environment.NewLine + jobExeEX.StackTrace, this);
            }
             catch(SchedulerException schedulerEx)
            {
                Log.Error(String.Format("Error Occured in {0}", schedulerEx.Source) + schedulerEx.Message + Environment.NewLine + schedulerEx.StackTrace, this);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
            }
        }

        private List<JobDetail> GetConfiguredJobs()
        {
            //get a list of all Quartz Scheduler Items including JobDetails and Triggers
            //string quartzJobsQuery = "fast://sitecore/content//*[@@templateid='{D01E915E-A1C2-4DB2-A2C8-B619513A82CB}']//*";
            string quartzJobsQuery = "fast://" + GetJobDefinitionLocation() + "//*[@@templateid='" + Templates.JobDetailTemplate + "']";

            Database masterDb = Factory.GetDatabase("master");
            Item[] quartzJobs = masterDb.SelectItems(quartzJobsQuery);


            List<JobDetail> lstJobs = new List<JobDetail>();

            #region Old Code
            //JobDetail jd = new JobDetail()
            //{
            //    JobKey = "myJob",
            //    Group = "Group1",
            //    Description = "Hello World Job",
            //    Type = "Scheduler.HelloJob, Scheduler"
            //};

            //var jobData = new Dictionary<string, Object>();
            //jobData.Add("path",@"D:\Test" );
            //jobData.Add("tablestoclean", "Log, ContentStatistics");
            //jobData.Add("value", 2);

            //JobDataMap jdMap = new JobDataMap((IDictionary<string, Object>) jobData);
            //jd.JobData = jdMap;

            //List<TriggerDetail> lstTriggers = new List<TriggerDetail>();

            /////TODO: Need to change this to set actual shedule
            //TriggerDetail td = new TriggerDetail()
            //{
            //    triggerKey = "Every10Seconds",
            //    RepeatFrequency = 10,
            //    IntervalUnit = RecurringInterval.Seconds,
            //    Interval = 5,
            //    StartTime = DateTime.Now
            //};
            #endregion

            if (quartzJobs != null && quartzJobs.Length > 0)
            {
                foreach (Item jobItem in quartzJobs)
                {
                    JobDetail jd = new JobDetail();
                    jd.ItemId = jobItem.ID.ToString();
                    jd.ItemName = jobItem.Name;
                    jd.Type = jobItem["Type"];
                    jd.Description = jobItem["Description"];
                    jd.JobKey = jobItem["Job Key"];
                    jd.Group = jobItem["Group"];
                    jd.JobData = jobItem["Job Data Map"];

                    lstJobs.Add(jd);
                } //end for loop for jobs
            }//end if

            return lstJobs;
        }

        private static string GetJobDefinitionLocation()
        {
            string sitecoreJobDefinitionLocation = ConfigurationManager.AppSettings.Get("Sitecore.QuartzScheduler.JobLocation");

            if (String.IsNullOrEmpty(sitecoreJobDefinitionLocation))
            {
                sitecoreJobDefinitionLocation = "sitecore/content";
            }

            return sitecoreJobDefinitionLocation;
        }

        private List<TriggerDetail> GetTriggersForJob(JobDetail jobDetail)
        {
            Database masterDb = Factory.GetDatabase("master");

            //Get the trigger definitions for this job
            string quartzJobTriggersQuery =
                "fast://" + GetJobDefinitionLocation() + "//*[@@parentid='" + jobDetail.ItemId + "']//*[@@templateid='" + Templates.TriggerDetailTempalte + "']";

            Item[] quartzJobTriggers = masterDb.SelectItems(quartzJobTriggersQuery);
            List<TriggerDetail> lstTriggers = new List<TriggerDetail>();

            if (quartzJobTriggers != null && quartzJobTriggers.Length > 0)
            {
               
                foreach (Item triggerItem in quartzJobTriggers)
                {
                    TriggerDetail triggerDetail = new TriggerDetail();
                    triggerDetail.TriggerKey = triggerItem["Trigger Key"];
                    if (!String.IsNullOrEmpty(triggerItem.Fields["Start Time"].Value))
                    {
                        triggerDetail.StartTime = ((DateField)triggerItem.Fields["Start Time"]).DateTime;
                    }
                    if (!String.IsNullOrEmpty(triggerItem.Fields["End Time"].Value))
                    {
                        triggerDetail.EndTime = ((DateField)triggerItem.Fields["End Time"]).DateTime;
                    }

                    //Handling Days of Week field values
                    //triggerDetail.DaysOfWeeks = triggerItem["Days of Week"];
                    MultilistField daysOfWeekField = triggerItem.Fields["Days of Week"];
                    if (daysOfWeekField != null)
                    {
                        Item[] daysOfWeek = daysOfWeekField.GetItems();
                        if (daysOfWeek != null && daysOfWeek.Length > 0)
                        {
                            foreach (Item day in daysOfWeek)
                            {
                                triggerDetail.DaysOfWeeks.Add((Sitecore.DaysOfWeek)Enum.Parse(typeof(Sitecore.DaysOfWeek), day.Name));
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(triggerItem.Fields["Day of Month"].Value))
                        triggerDetail.DayOfMonth = int.Parse(triggerItem["Day of Month"]);

                    //if (!String.IsNullOrEmpty(triggerItem.Fields["Repeat Every"].Value))
                    //    triggerDetail.IntervalUnit = triggerItem["Repeat Every"];


                    if (!String.IsNullOrEmpty(triggerItem.Fields["Repeat Interval"].Value))
                        triggerDetail.RepeatInterval = int.Parse(triggerItem["Repeat Interval"]);


                    if (!String.IsNullOrEmpty(triggerItem.Fields["Repeat Count"].Value))
                        triggerDetail.RepeatCount = int.Parse(triggerItem["Repeat Count"]);

                    if (!String.IsNullOrEmpty(triggerItem.Fields["Schedule Type"].Value))
                    {
                        triggerDetail.ScheduleType = triggerItem["Schedule Type"];
                        triggerDetail.ScheduleTypeValue = masterDb.GetItem(new ID(triggerDetail.ScheduleType)).Name;
                    }

                    if (!String.IsNullOrEmpty(triggerItem.Fields["Cron Expression"].Value))
                        triggerDetail.CronExpression = triggerItem["Cron Expression"];

                    lstTriggers.Add(triggerDetail);
                } //end for loop for triggers
            } //end if

            return lstTriggers;
        }

        public IList<string> GetJobGroupNames()
        {
            return scheduler.GetJobGroupNames();
        }

        public List<JobDetail> GetAllJobs()
        {

            List<JobDetail> jobsList = new List<JobDetail>();

            IList<string> jobGroups = GetJobGroupNames();

            foreach (string group in jobGroups)
            {
                jobsList.AddRange(GetJobDetailsInGroup(group));
            }

            return jobsList;
        }

        public List<JobDetail> GetJobDetailsInGroup(string group)
        {
            var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
            var jobKeys = scheduler.GetJobKeys(groupMatcher);
            List<JobDetail> jobList = new List<JobDetail>();

            foreach (var jobKey in jobKeys)
            {
                var iJobDetail = scheduler.GetJobDetail(jobKey);
                JobDetail jd = new JobDetail();
                jd.JobKey = iJobDetail.Key.Name;
                jd.Group = iJobDetail.Key.Group;
                jd.Type = iJobDetail.JobType.ToString();
                jd.Description = iJobDetail.Description;
                jd.JobData = JobManagerUtility.GetJobDataMapSitecoreString(iJobDetail.JobDataMap);
                
                jobList.Add(jd);
            }
            return jobList;
        }

        public List<TriggerDetail> GetJobTriggers(string jobKey)
        {
            var triggers = scheduler.GetTriggersOfJob(new JobKey(jobKey));
            List<TriggerDetail> triggerDetailList = new List<TriggerDetail>();

            foreach (ITrigger t in triggers)
            {
                TriggerDetail trigger = new TriggerDetail();
                trigger.TriggerKey = t.Key.Name;
                triggerDetailList.Add(trigger);
            }

            return triggerDetailList;

        }

        public void ExecuteJob(string jobKey)
        {
            Log.Info(String.Format("Executing Job {0} at {1} triggered by user {2} on demand", jobKey, DateTime.Now, Sitecore.Context.User.Name), this);
            IJobDetail jobDetail = scheduler.GetJobDetail(new JobKey(jobKey));

            ITrigger trigger = TriggerBuilder.
                Create().
                ForJob(jobDetail).
                WithIdentity("ondemand", jobDetail.Key.Group).
                WithSchedule(SimpleScheduleBuilder.Create().WithRepeatCount(0).WithInterval(TimeSpan.Zero)).
                StartNow().Build();
            scheduler.ScheduleJob(jobDetail, trigger);
            Log.Info(String.Format("Job {0} completed at {1} triggered by user {2} on demand", jobKey, DateTime.Now, Sitecore.Context.User.Name), this);
        }

        public DateTime GetNextFireTime(string group, string triggerKey)
        {
            DateTime nextFireTime = DateTime.MinValue;
            ITrigger trigger = scheduler.GetTrigger((new TriggerKey(triggerKey, group)));
            if (trigger.GetNextFireTimeUtc().HasValue)
                nextFireTime = trigger.GetNextFireTimeUtc().Value.DateTime.ToLocalTime();

            return nextFireTime;
        }

        public DateTime? GetPreviousFireTime(string group, string triggerKey)
        {
            DateTime? previousFireTime = null;
            ITrigger trigger = scheduler.GetTrigger((new TriggerKey(triggerKey, group)));

            if (trigger.GetPreviousFireTimeUtc().HasValue)
                previousFireTime = trigger.GetPreviousFireTimeUtc().Value.DateTime.ToLocalTime();

            return previousFireTime;
        }

        public string GetTriggerState(string group, string triggerKey)
        { 
            return scheduler.GetTriggerState(new TriggerKey(triggerKey, group)).ToString();
        }


    }
}
