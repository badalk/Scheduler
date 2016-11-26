using Quartz;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.QuartzScheduler.Models;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Sitecore.QuartzScheduler.Repository
{
    public class TriggerDetailRepository : Sitecore.Services.Core.IRepository<TriggerDetail>
    {
        public void Add(TriggerDetail entity)
        {
            using (new SecurityDisabler())
            {
                Database masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
                Item parentItem = masterDB.GetItem(entity.ParentItemId);

                //Get triggers folder under the job item
                Item triggerFolder = EnsureTriggerFolder(masterDB, parentItem);
                TemplateItem triggerTemplate = masterDB.GetTemplate(ID.Parse(Common.Constants.TriggerDetailTempalteID));
                //"/sitecore/templates/modules/quartzscheduler/jobdetail");
                Item triggerItem = triggerFolder.Add(entity.TriggerKey, triggerTemplate);
                triggerItem.Editing.BeginEdit();
                try
                {
                    UpdateFields(entity, triggerItem);
                }
                catch (Exception ex)
                {
                    Sitecore.Diagnostics.Log.Info(String.Format("Error adding Trigger information for {0}", String.IsNullOrEmpty(entity.TriggerKey)), this);
                    Sitecore.Diagnostics.Log.Error("Sitecore.QuartzScheuler: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
                }
                finally
                {
                    triggerItem.Editing.EndEdit();
                }


            }
        }

        private Item EnsureTriggerFolder(Database masterDB, Item parentItem)
        {
            Item triggersFolderItem = null;
            try
            {
                var triggerFolders = parentItem.GetChildren();
                if (triggerFolders != null && triggerFolders.Count > 0)
                    triggersFolderItem = parentItem.GetChildren().FirstOrDefault();
                else
                {
                    TemplateItem triggerFolderTemplateItem = masterDB.GetTemplate(ID.Parse(Common.Constants.TriggersFolderTemplateID));
                    triggersFolderItem = parentItem.Add("Triggers", triggerFolderTemplateItem);
                }
            }
            catch (Exception ex) // this means there is no triggers folder - so create one
            {
                Sitecore.Diagnostics.Log.Error("Sitecore.QuartzScheuler: Error occured while creating Triggers folder : " + ex.Message + Environment.NewLine + ex.StackTrace, this);
            }
            return triggersFolderItem;
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
                Sitecore.Diagnostics.Log.Error(String.Format("Sitecore.QuartzScheuler: Could not find an item to delete. Item {0} does not exist", entity.itemId), this);
                Sitecore.Diagnostics.Log.Error("Sitecore.QuartzScheuler: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
            }
        }

        public bool Exists(TriggerDetail entity)
        {
            if (String.IsNullOrEmpty(entity.itemId))
                return false;
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
                    TriggerKey = triggerDetail["Trigger Key"]
                };
                trigger.StartTime = DateUtil.IsoDateToDateTime(triggerDetail.Fields["Start Time"].Value);
                trigger.EndTime = DateUtil.IsoDateToDateTime(triggerDetail.Fields["End Time"].Value);

                //ScheduleType
                ReferenceField scheduleType = triggerDetail.Fields["Schedule Type"];
                if (scheduleType != null && scheduleType.TargetItem != null)
                {
                    trigger.ScheduleType = scheduleType.TargetItem.ID.ToString();
                }

                trigger.DaysOfWeeks = GetDaysOfWeek(triggerDetail.Fields["Days of Week"]);

                //DayofMonth
                if (!string.IsNullOrEmpty(triggerDetail.Fields["Day of Month"].Value))
                    trigger.DayOfMonth = int.Parse(triggerDetail.Fields["Day of Month"].Value);

                //Repeat Interval
                if (!string.IsNullOrEmpty(triggerDetail.Fields["Repeat Interval"].Value))
                    trigger.RepeatInterval = int.Parse(triggerDetail.Fields["Repeat Interval"].Value);

                //Cron Expression
                trigger.CronExpression = triggerDetail.Fields["Cron Expression"].Value;


                return trigger;
            }
            else
            {
                return null;
            }
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

        public IQueryable<TriggerDetail> GetAll()
        {
            //get a list of all Quartz Scheduler Items including JobDetails and Triggers
            string triggerDefinitionsQuery = "fast://" + GetJobDefinitionLocation() + "//*[@@templateid='" + Common.Constants.TriggerDetailTempalteID + "']";

            Database masterDb = Factory.GetDatabase("master");
            Item[] triggers = masterDb.SelectItems(triggerDefinitionsQuery);


            List<TriggerDetail> lstTriggers = new List<TriggerDetail>();


            if (triggers != null && triggers.Length > 0)
            {
                foreach (Item t in triggers)
                {
                    TriggerDetail td = new TriggerDetail();
                    td.Id = t.ID.ToString();
                    td.TriggerKey = t.Fields["Trigger Key"].Value;
                    td.StartTime = DateUtil.IsoDateToDateTime(t.Fields["Start Time"].Value);
                    td.EndTime = DateUtil.IsoDateToDateTime(t.Fields["End Time"].Value);
                    td.ScheduleType = t.Fields["Schedule Type"].Value;
                    td.RepeatInterval = (String.IsNullOrEmpty(t.Fields["Repeat Interval"].Value) ? 0 : int.Parse(t.Fields["Repeat Interval"].Value));
                    td.RepeatCount = (String.IsNullOrEmpty(t.Fields["Repeat Count"].Value) ? 0 : int.Parse(t.Fields["Repeat Count"].Value));
                    td.DayOfMonth = (String.IsNullOrEmpty(t.Fields["Day of Month"].Value) ? 0 : int.Parse(t.Fields["Day of Month"].Value));
                    td.CronExpression = t.Fields["Cron Expression"].Value;
                    td.DaysOfWeeks = GetDaysOfWeek(t.Fields["Days of Week"]);

                    lstTriggers.Add(td);
                } //end for loop for triggers
            }//end if

            return lstTriggers.AsQueryable<TriggerDetail>();


        }

        private static List<Models.DayOfWeek> GetDaysOfWeek(MultilistField daysOfWeeks)
        {
            List<Models.DayOfWeek> lstDaysOfWeek = new List<Models.DayOfWeek>();
            if (daysOfWeeks != null)
            {
                // trigger.DaysOfWeeks = daysOfWeeks.GetItems().ToList<Item>();
                Item[] items = daysOfWeeks.GetItems();
                if (items != null && items.Length > 0)
                {

                    for (int i = 0; i < items.Length; i++)
                    {
                        Models.DayOfWeek dayOfWeek = new Models.DayOfWeek()
                        {
                            itemId = items[i].ID.ToString(),
                            itemName = items[i].Name,
                            DayOfWeekValue = (DaysOfWeek)Enum.Parse(typeof(DaysOfWeek), items[i].Name, true)
                        };
                        lstDaysOfWeek.Add(dayOfWeek);
                    }
                }
            }
            return lstDaysOfWeek;
        }

        public void Update(TriggerDetail entity)
        {
            using (new SecurityDisabler())
            {
                Database masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
                TemplateItem triggerTemplate = masterDB.GetTemplate(ID.Parse(Common.Constants.TriggerDetailTempalteID));
                //"/sitecore/templates/modules/quartzscheduler/TriggerDetail");
                Item triggerItem = masterDB.GetItem(new ID(entity.itemId));
                triggerItem.Editing.BeginEdit();
                try
                {
                    UpdateFields(entity, triggerItem);
                }
                catch (Exception ex)
                {
                    Sitecore.Diagnostics.Log.Info(String.Format("Sitecore.QuartzScheuler: Error updating Trigger information for {0}", String.IsNullOrEmpty(entity.TriggerKey)), this);
                    Sitecore.Diagnostics.Log.Error("Sitecore.QuartzScheuler: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
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
            DateField startTimeField = triggerItem.Fields["Start Time"];
            if (startTimeField != null)
            {
                startTimeField.Value = Sitecore.DateUtil.ToIsoDate(entity.StartTime);
            }
            triggerItem.Fields["Start Time"].Value = DateUtil.ToIsoDate(entity.StartTime);
            if (entity.EndTime != null || entity.EndTime != DateTime.MinValue)
                triggerItem.Fields["End Time"].Value = DateUtil.ToIsoDate(entity.EndTime);
            else
                triggerItem.Fields["End Time"].Value = "";

            triggerItem.Fields["Schedule Type"].Value = entity.ScheduleType;
            triggerItem.Fields["Day of Month"].Value = entity.DayOfMonth.ToString();
            triggerItem.Fields["Repeat Count"].Value = entity.RepeatCount.ToString();
            triggerItem.Fields["Repeat Interval"].Value = entity.RepeatInterval.ToString();
            triggerItem.Fields["Cron Expression"].Value = entity.CronExpression;
            MultilistField daysOfWeeks = triggerItem.Fields["Days Of Week"];
            Item[] selectedDays = daysOfWeeks.GetItems();
            for (int x = 0; x < selectedDays.Length ; x++)
            {
                daysOfWeeks.Remove(selectedDays[x].ID.ToString());
            }
            for (int i = 0; i < entity.DaysOfWeeks.Count; i++)
            {
                daysOfWeeks.Add(entity.DaysOfWeeks[i].itemId);
            }
        }

    }
}