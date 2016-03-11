using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.QuartzScheduler.Models;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.QuartzScheduler.Repository
{
    public class TriggerDetailRepository : Sitecore.Services.Core.IRepository<TriggerDetail>
    {
        public void Add(TriggerDetail entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(TriggerDetail entity)
        {
            Database masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
            try
            {
                Item triggerItem = masterDB.GetItem(new ID(entity.itemId));
                triggerItem.Delete();
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(String.Format("Could not find an item to delete. Item {0} does not exist", entity.itemId), this);
                Sitecore.Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
            }
        }

        public bool Exists(TriggerDetail entity)
        {
            var triggerDetail = Sitecore.Data.Database.GetDatabase("master").GetItem(new ID(entity.itemId));
            return triggerDetail != null;
        }

        public TriggerDetail FindById(string id)
        {
            Log.Info("Calling FindById on TriggerDetail Entity Service Repository with Trigger Id : " + id, this);

            var triggerDetail = Sitecore.Data.Database.GetDatabase("master").GetItem(new ID(id));

            if (triggerDetail != null)
            {
                TriggerDetail trigger = new TriggerDetail()
                {
                    Id = triggerDetail.ID.ToString(),
                    TriggerKey = triggerDetail["Trigger Key"],
                    StartTime = triggerDetail["Start Time"],
                    EndTime = triggerDetail["End Time"],
                    ScheduleType = triggerDetail["Schedule Type"],
                    DaysOfWeeks = triggerDetail["Days of Week"],
                    DayOfMonth = triggerDetail["Day of Month"],
                    RepeatCount = triggerDetail["Repeat Count"],
                    RepeatInterval = triggerDetail["Repeat Interval"],
                    CronExpression = triggerDetail["Cron Expression"]
                };

                ////Get Job Data Map
                //NameValueCollection jobData = Sitecore.Web.WebUtil.ParseUrlParameters(jobDetail["Job Data Map"]);

                //if (jobData != null)
                //{
                //    jd.JobData = new JobDataMap();
                //    foreach (string key in jobData.Keys)
                //    {
                //        jd.JobData.Add(key, jobData[key]);
                //    }
                //}

                return trigger;
            }
            else
            {
                return null;
            }
        }

        public IQueryable<TriggerDetail> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Update(TriggerDetail entity)
        {
            using (new SecurityDisabler())
            {
                Database masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
                TemplateItem triggerTemplate = masterDB.GetTemplate(ID.Parse("{822F8EF2-16CE-4B31-AD36-4F2132D81A39}"));
                //"/sitecore/templates/modules/quartzscheduler/TriggerDetail");
                Item triggerItem = masterDB.GetItem(new ID(entity.itemId));
                triggerItem.Editing.BeginEdit();
                try
                {
                    UpdateFields(entity, triggerItem);
                }
                catch (Exception ex)
                {
                    Sitecore.Diagnostics.Log.Info(String.Format("Error updating Job information for {0}", String.IsNullOrEmpty(entity.JobKey)), this);
                    Sitecore.Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);

                }
                finally
                {
                    triggerItem.Editing.EndEdit();
                }

            }


        }

        private void UpdateFields(TriggerDetail entity, Item triggerItem)
        {
            triggerItem.Fields["Trigger Key"].Value = entity.TriggerKey;
            triggerItem.Fields["Start Time"].Value = entity.StartTime;
            triggerItem.Fields["End Time"].Value = entity.EndTime;
            triggerItem.Fields["Schedule Type"].Value = entity.ScheduleType;
            triggerItem.Fields["Days of Week"].Value = entity.DaysOfWeeks;
            triggerItem.Fields["Day of Month"].Value = entity.DayOfMonth;
            triggerItem.Fields["Repeat Count"].Value = entity.RepeatCount;
            triggerItem.Fields["Repeat Interval"].Value = entity.RepeatInterval;
            triggerItem.Fields["Cron Expression"].Value = entity.CronExpression;
        }
    }
}