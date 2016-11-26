using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.QuartzScheduler.Models;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Sitecore.Diagnostics;
using Sitecore.Configuration;

namespace Sitecore.QuartzScheduler.Repository
{
    /// <summary>
    /// JobDetailRepository class deals as DataAccess layer with Sitecore Items. It allows you to add / update / delete / find JobDetail entities from Sitecore.
    /// This is a standard repository implementation recommended by Sitecore from Sitecore.Client.Services documentation.
    /// </summary>
    public class JobDetailRepository : Sitecore.Services.Core.IRepository<JobDetail>
    {
        public void Add(JobDetail entity)
        {
            try
            {
                using (new SecurityDisabler())
                {
                    string sitecoreJobDefinitionLocation = Settings.GetSetting("Sitecore.QuartzScheduler.JobLocation");

                    if (!String.IsNullOrEmpty(sitecoreJobDefinitionLocation))
                    {
                        Database masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
                        Item parentItem = masterDB.GetItem(@"/" + sitecoreJobDefinitionLocation + @"/");
                        TemplateItem jdTemplate = masterDB.GetTemplate(ID.Parse(Common.Constants.JobDetailTemplateID));
                        //"/sitecore/templates/modules/quartzscheduler/jobdetail");
                        if (parentItem == null)
                            Log.Info("Sitecore.QuartzScheduler.JobDefinitionLocation does not exist", this);
                        Item jdItem = parentItem.Add(entity.JobKey, jdTemplate);
                        jdItem.Editing.BeginEdit();
                        try
                        {
                            UpdateFields(entity, jdItem);
                        }
                        catch (Exception ex)
                        {
                            Sitecore.Diagnostics.Log.Info(String.Format("Error adding Job information for {0}", String.IsNullOrEmpty(entity.JobKey)), this);
                            Sitecore.Diagnostics.Log.Error("Sitecore.QuartzScheuler: " + ex.Message + Environment.NewLine + ex.StackTrace, this);

                        }
                        finally
                        {
                            jdItem.Editing.EndEdit();
                        }

                    }


                }
            }
            catch(Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
        }

        private NameValueCollection GetNameValueCollection(JobDataMap jobData)
        {
            NameValueCollection nvCollection = new NameValueCollection();
            if (jobData != null && jobData.Keys != null && jobData.Keys.Count > 0)
            {
                foreach (var key in jobData.Keys)
                {
                    nvCollection.Add(key, jobData.GetString(key));
                }
            }
            return nvCollection;
        }

        public void Delete(JobDetail entity)
        {
            Database masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
            try
            {
                Item jdItem = masterDB.GetItem(new ID(entity.itemId));
                jdItem.Delete();
            }
            catch(Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(String.Format("Sitecore.QuartzScheuler: Could not find an item to delete. Item {0} does not exist", entity.itemId), this);
                Sitecore.Diagnostics.Log.Error("Sitecore.QuartzScheuler: " + ex.Message + Environment.NewLine + ex.StackTrace, this);
            }
       }

        public bool Exists(JobDetail entity)
        {
            if (string.IsNullOrEmpty(entity.itemId))
                return false;

            var jobDetail = Sitecore.Data.Database.GetDatabase("master").GetItem(new ID(entity.itemId));

            return jobDetail != null;
        }



        public JobDetail FindById(string id)
        {
            var jobManager = new JobManager();
            return jobManager.GetJobDetails(id);
        }

        public IQueryable<JobDetail> GetAll()
        {
            var jobManager = new JobManager();
            return jobManager.GetAllJobs().AsQueryable<JobDetail>(); ;
        }


        public void Update(JobDetail entity)
        {
            try
            {
                using (new SecurityDisabler())
                {
                    Database masterDB = Sitecore.Configuration.Factory.GetDatabase("master");
                    TemplateItem jdTemplate = masterDB.GetTemplate(ID.Parse("{C57D9C9A-BA63-4C3E-BFD5-4823B20BB5AE}"));
                    //"/sitecore/templates/modules/quartzscheduler/jobdetail");
                    Item jdItem = masterDB.GetItem(new ID(entity.itemId));
                    jdItem.Editing.BeginEdit();
                    try
                    {
                        UpdateFields(entity, jdItem);
                    }
                    catch (Exception ex)
                    {
                        Sitecore.Diagnostics.Log.Info(String.Format("Sitecore.QuartzScheuler: Error updating Job information for {0}", entity.JobKey), this);
                        Sitecore.Diagnostics.Log.Error("Sitecore.QuartzScheuler: " + ex.Message + Environment.NewLine + ex.StackTrace, this);

                    }
                    finally
                    {
                        jdItem.Editing.EndEdit();
                    }

                }
            }
            catch(Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, this);
                throw ex;
            }
        }

        private void UpdateFields(JobDetail entity, Item jdItem)
        {
            jdItem.Fields["Type"].Value = entity.Type;
            jdItem.Fields["Job Key"].Value = entity.JobKey;
            jdItem.Fields["Description"].Value = entity.Description;
            jdItem.Fields["Group"].Value = entity.Group;
            jdItem.Fields["Job Data Map"].Value = entity.JobData;
        }
    }
}
