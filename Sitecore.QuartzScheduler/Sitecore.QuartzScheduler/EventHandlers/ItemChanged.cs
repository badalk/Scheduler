using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.QuartzScheduler.EventHandlers
{
    /// <summary>
    /// This eventhandler handles item saved and item deleted events and re-schedule the events in those situations only when items for QuartzScheduler templates are changed.
    /// </summary>
    public class ItemChanged
    {
        /// <summary>
        /// Same method is called in ItemSaved and ItemDeleted pipeline based on the configuration
        /// </summary>
        /// <param name="sender">An item on which the event has occured</param>
        /// <param name="args">Event arguments</param>
        public void OnItemChanged(object sender, EventArgs args)
        {
            Item changedItem = Event.ExtractParameter(args, 0) as Item;

            if (changedItem != null && changedItem.Database.Name.ToLower() == "master")
            {
                //Check for all the templates used for QuartzScheduler
                if (changedItem.TemplateID == ID.Parse("{822F8EF2-16CE-4B31-AD36-4F2132D81A39}") || 
                    changedItem.TemplateID == ID.Parse("{C57D9C9A-BA63-4C3E-BFD5-4823B20BB5AE}") )
                {
                    JobManager scheduler = new JobManager();
                    scheduler.ScheduleJobs();
                }
            }
        }
    }
}
