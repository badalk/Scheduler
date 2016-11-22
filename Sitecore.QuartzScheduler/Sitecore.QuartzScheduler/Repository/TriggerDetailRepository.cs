using Quartz;
using Sitecore.Data;
using Sitecore.Data.Fields;
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
            using (new SecurityDisabler())
            {
                Database masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
                Item parentItem = masterDB.GetItem(entity.ParentItemId);
                TemplateItem triggerTemplate = masterDB.GetTemplate(ID.Parse("{822F8EF2-16CE-4B31-AD36-4F2132D81A39}"));
                //"/sitecore/templates/modules/quartzscheduler/jobdetail");
                Item triggerItem = parentItem.Add(entity.TriggerKey, triggerTemplate);
                triggerItem.Editing.BeginEdit();
                try
                {
                    UpdateFields(entity, triggerItem);
                }
                catch (Exception ex)
                {
                    Sitecore.Diagnostics.Log.Info(String.Format("Error adding Trigger information for {0}", String.IsNullOrEmpty(entity.TriggerKey)), this);
                    Sitecore.Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                }
                finally
                {
                    triggerItem.Editing.EndEdit();
                }


            }
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
                trigger.StartTime = ((DateField) triggerDetail.Fields["Start Time"]).DateTime;
                trigger.EndTime = ((DateField)triggerDetail.Fields["End Time"]).DateTime;

                //ScheduleType
                ReferenceField scheduleType = triggerDetail.Fields["Schedule Type"];
                if (scheduleType != null && scheduleType.TargetItem != null)
                {
                    trigger.ScheduleType = scheduleType.TargetItem.ID.ToString();
                }


                //DaysofWeek
                MultilistField daysOfWeeks = triggerDetail.Fields["Days of Week"];
                if (daysOfWeeks != null)
                {
                    // trigger.DaysOfWeeks = daysOfWeeks.GetItems().ToList<Item>();
                    Item[] items = daysOfWeeks.GetItems();
                    if (items != null && items.Length > 0)
                    {
                        List<Models.DayOfWeek> lstDaysOfWeek = new List<Models.DayOfWeek>();

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
                        trigger.DaysOfWeeks = lstDaysOfWeek;
                    }
                }

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

        public IQueryable<TriggerDetail> GetAll()
        {
            throw new NotImplementedException();
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
                    Sitecore.Diagnostics.Log.Info(String.Format("Error updating Trigger information for {0}", String.IsNullOrEmpty(entity.TriggerKey)), this);
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
            DateField startTimeField = triggerItem.Fields["Start Time"];
            if (startTimeField != null)
            {
                startTimeField.Value = Sitecore.DateUtil.ToIsoDate(entity.StartTime);
            }
            triggerItem.Fields["Start Time"].Value = entity.StartTime.ToString();
            triggerItem.Fields["End Time"].Value = entity.EndTime.ToString();
            triggerItem.Fields["Schedule Type"].Value = entity.ScheduleType;
            triggerItem.Fields["Day of Month"].Value = entity.DayOfMonth.ToString();
            triggerItem.Fields["Repeat Count"].Value = entity.RepeatCount.ToString();
            triggerItem.Fields["Repeat Interval"].Value = entity.RepeatInterval.ToString();
            triggerItem.Fields["Cron Expression"].Value = entity.CronExpression;
        }


    }
}