﻿using Quartz;
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
using System.Linq;

namespace Sitecore.QuartzScheduler
{
    public class JobManager
    {
        static IScheduler scheduler;

        public JobManager()
        {
            // get a scheduler
            if (scheduler == null)
            {
                scheduler = StdSchedulerFactory.GetDefaultScheduler();
                Log.Info("Scheduler Instance ID: " + scheduler.SchedulerInstanceId, this);
                scheduler.ListenerManager.AddSchedulerListener(new SchedulerListener());
                scheduler.ListenerManager.AddJobListener(new SchedulerJobListener(), GroupMatcher<JobKey>.AnyGroup());
            }

        }

        public JobDetail GetJobDetails(string jobID)
        {
            Log.Info("Calling FindById on JobDetail Entity Service Repository with Job Id : " + jobID, this);

            var jobDetail = Sitecore.Data.Database.GetDatabase("master").GetItem(new ID(jobID));

            if (jobDetail != null)
            {
                JobDetail jd = new JobDetail()
                {
                    Id = jobDetail.ID.ToString(),
                    Description = jobDetail["Description"],
                    JobKey = jobDetail["Job Key"],
                    Type = jobDetail["Type"],
                    Group = jobDetail["Group"],
                    JobData = jobDetail["Job Data Map"]
                };

                return jd;
            }
            else
            {
                return null;
            }

        }

        public void ScheduleJobs()
        {
            try
            {

                //start a scheduler
                var schedulerMetaData = scheduler.GetMetaData();
                Log.Info("Scheduler Metadata Summary =>", this);
                Log.Info(schedulerMetaData.GetSummary(), this);
                Log.Info("SchedulerInstanceId : " + schedulerMetaData.SchedulerInstanceId, this);
                Log.Info("SchedulerName : " + schedulerMetaData.SchedulerName, this);
                Log.Info("ThreadPoolSize : " + schedulerMetaData.ThreadPoolSize, this);
                Log.Info("ThreadPoolType: " + schedulerMetaData.ThreadPoolType.ToString(), this);
                Log.Info("JobStoreType: " + schedulerMetaData.JobStoreType.ToString(), this);

                scheduler.Start();

                //Create Job Definitions and add it to the collection to simulate this is what we will get from Sitecore
                Log.Info("Getting Job Definitions", this);
                var jobDefinitions = GetConfiguredJobs();

                //Cleanup any existing jobs scheduled already 
                //to ensure that every time ScheduleJobs is called, 
                //it schedules the whole list of jobs from fresh
                Log.Info("Clearing existing scheduled Jobs before we set it up agin !", this);
                scheduler.Clear();

                if (jobDefinitions != null && jobDefinitions.Count > 0)
                {
                    foreach (JobDetail jd in jobDefinitions)
                    {
                        //if (jd.JobKey.Equals("Cleanup Publish Queue"))
                        //{

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

                        ITriggerListener trigListener = new SchedulerTriggerListener();

                        #region "Looping through trigger Details"

                        foreach (TriggerDetail td in triggersForJob)
                        {
                            TriggerBuilder trigger = GetTriggerBuilder(jd, td);
                            //Add Job and Trigger listeners
                            Log.Info("Registering Trigger Listener", this);
                            scheduler.ListenerManager.AddTriggerListener(trigListener, EverythingMatcher<JobKey>.AllTriggers());
                            triggers.Add(trigger.Build());
                            Log.Info(String.Format("Job {0} is scheduled with trigger {1}.", jd.JobKey, td.TriggerKey), this);


                        }
                        #endregion

                        scheduler.ScheduleJob(job, triggers, true);
                    }
                }
                else
                {
                    Log.Info("No Jobs found. No jobs will be scheduled.", this);
                }
                //}
            }
            catch (JobExecutionException jobExeEX)
            {
                Log.Error(String.Format("Error Occured in {0}", jobExeEX.Source) + jobExeEX.Message + Environment.NewLine + jobExeEX.StackTrace, this);
            }
            catch (SchedulerException schedulerEx)
            {
                Log.Error(String.Format("Error Occured in {0}", schedulerEx.Source) + schedulerEx.Message + Environment.NewLine + schedulerEx.StackTrace, this);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
            }
        }

        private TriggerBuilder GetTriggerBuilder(JobDetail jd, TriggerDetail td)
        {
            var trigger = TriggerBuilder.Create();
            trigger.WithIdentity(td.TriggerKey, jd.Group);
            trigger.ForJob(jd.JobKey);
            trigger.WithPriority(td.Priority);
            trigger.StartAt(td.StartTime); 
            if (td.EndTime != null && !td.EndTime.Equals(DateTime.MinValue))
                trigger.EndAt(td.EndTime);

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
                    if (td.DaysOfWeeks != null && td.DaysOfWeeks.Count > 0)
                    {
                        System.DayOfWeek[] dayOfWeeks = new System.DayOfWeek[td.DaysOfWeeks.Count];
                        for (int i = 0; i < td.DaysOfWeeks.Count; i++)
                        {
                            dayOfWeeks[i] = (System.DayOfWeek)Enum.Parse(typeof(System.DayOfWeek), td.DaysOfWeeks[i].DayOfWeekValue.ToString());
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


            return trigger;
        }

        public List<JobDetail> GetConfiguredJobs()
        {
            List<JobDetail> lstJobs = new List<JobDetail>();
            //get a list of all Quartz Scheduler Items including JobDetails and Triggers
            string jobsDefinitionsQuery = "fast://" + GetJobDefinitionLocation() + "//*[@@templateid='" + Common.Constants.JobDetailTemplateID + "']";

            try
            {
                Database masterDb = Factory.GetDatabase("master");
                Item[] quartzJobs = masterDb.SelectItems(jobsDefinitionsQuery);



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
                        jd.Id = jobItem.ID.ToString();
                        jd.ItemName = jobItem.Name;
                        jd.Type = jobItem["Type"];
                        jd.Description = jobItem["Description"];
                        jd.JobKey = jobItem["Job Key"];
                        jd.Group = jobItem["Group"];
                        jd.JobData = jobItem["Job Data Map"];

                        lstJobs.Add(jd);
                    } //end for loop for jobs
                }//end if
            }
            catch(Exception ex)
            {
                Log.Error("Error Occured in JobManager.GetConfiguredJobs : " + ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
            return lstJobs.OrderBy(x => x.Group).ThenBy(x => x.JobKey).ToList();
        }

        private static string GetJobDefinitionLocation()
        {
            string sitecoreJobDefinitionLocation = Settings.GetSetting("Sitecore.QuartzScheduler.JobLocation");

            if (String.IsNullOrEmpty(sitecoreJobDefinitionLocation))
            {
                sitecoreJobDefinitionLocation = "sitecore/content";
            }

            return sitecoreJobDefinitionLocation;
        }

        public List<TriggerDetail> GetTriggersForJob(JobDetail jobDetail)
        {
            Database masterDb = Factory.GetDatabase("master");

            //Get the trigger definitions for this job
            string quartzJobTriggersQuery =
                "fast://" + GetJobDefinitionLocation() + "//*[@@parentid='" + jobDetail.Id + "']//*[@@templateid='" + Common.Constants.TriggerDetailTempalteID + "']";

            Item[] quartzJobTriggers = masterDb.SelectItems(quartzJobTriggersQuery);
            List<TriggerDetail> lstTriggers = new List<TriggerDetail>();

            if (quartzJobTriggers != null && quartzJobTriggers.Length > 0)
            {
               
                foreach (Item triggerItem in quartzJobTriggers)
                {
                    TriggerDetail triggerDetail = new TriggerDetail();
                    triggerDetail.Id = triggerItem.ID.ToString();
                    triggerDetail.ParentItemId = jobDetail.Id;
                    triggerDetail.TriggerKey = triggerItem["Trigger Key"];
                    if (!String.IsNullOrEmpty(triggerItem.Fields["Start Time"].Value))
                    {
                        triggerDetail.StartTime = ((DateField)triggerItem.Fields["Start Time"]).DateTime;
                    }
                    if (!String.IsNullOrEmpty(triggerItem.Fields["End Time"].Value))
                    {
                        triggerDetail.EndTime = ((DateField)triggerItem.Fields["End Time"]).DateTime;
                    }
                    if (!String.IsNullOrEmpty(triggerItem.Fields["Priority"].Value))
                    {
                        triggerDetail.Priority = int.Parse(triggerItem.Fields["Priority"].Value);
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
                        triggerDetail.ScheduleType = triggerItem.Fields["Schedule Type"].Value;
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


        public void ExecuteJob(string jobKey, string group)
        {
            try
            {
                Log.Info(String.Format("Executing Job {0} at {1} triggered by user {2} on demand", jobKey, DateTime.Now, Sitecore.Context.User.Name), this);
                IJobDetail jobDetail = scheduler.GetJobDetail(new JobKey(jobKey, group));

                scheduler.TriggerJob(new JobKey(jobKey, group), jobDetail.JobDataMap);
                Log.Info(String.Format("Job {0} completed at {1} triggered by user {2} on demand", jobKey, DateTime.Now, Sitecore.Context.User.Name), this);
            }
            catch(Exception ex)
            {
                Log.Error(String.Format("Error occured while executing Job \"{0}\" at {1} triggered by user {2} on demand", jobKey, DateTime.Now, Sitecore.Context.User.Name), this);
                Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
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

        private DateTime? GetLocalDateTime(DateTimeOffset? fireTime)
        {
            if (fireTime.HasValue)
                return fireTime.Value.LocalDateTime;
            else
                return null;
        }

        public List<JobExecutionStatus> GetCurrentJobStatus()
        {
            var executingJobs = scheduler.GetCurrentlyExecutingJobs();
            List<JobExecutionStatus> jobStatusList = new List<JobExecutionStatus>();

            foreach(var exJob in executingJobs)
            {
                var jobStatus = new JobExecutionStatus();

                jobStatus.JobKey = exJob.JobDetail.Key.Name;
                jobStatus.TriggerName = exJob.Trigger.Key.Name;
                jobStatus.FireTime = GetLocalDateTime(exJob.FireTimeUtc);
                jobStatus.PreviousFireTime = GetLocalDateTime(exJob.PreviousFireTimeUtc);
                jobStatus.ScheduledFireTime = GetLocalDateTime(exJob.ScheduledFireTimeUtc);
                jobStatus.NextFireTime = GetLocalDateTime(exJob.NextFireTimeUtc);
                jobStatus.JobRunTime = exJob.JobRunTime.TotalSeconds;
                jobStatus.State = scheduler.GetTriggerState(exJob.Trigger.Key).ToString();

                jobStatusList.Add(jobStatus);
            }


            return jobStatusList;
        }

        public List<JobExecutionStatus> GetJobFireTimes()
        {
            //TODO: Get next fire time for jobs which are not currently running
            List<JobExecutionStatus> jobStatusList = new List<JobExecutionStatus>();
            var jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var job in jobKeys)
            {
                var triggers = scheduler.GetTriggersOfJob(job);

                foreach (var trigger in triggers)
                {
                    var jobStatus = new JobExecutionStatus();
                    jobStatus.JobKey = job.Name;
                    jobStatus.TriggerName = trigger.Key.Name;
                    jobStatus.PreviousFireTime = GetLocalDateTime(trigger.GetPreviousFireTimeUtc());
                    jobStatus.NextFireTime = GetLocalDateTime(trigger.GetNextFireTimeUtc());

                    jobStatusList.Add(jobStatus);
                }
            }

            return jobStatusList;

        }

    }


}
