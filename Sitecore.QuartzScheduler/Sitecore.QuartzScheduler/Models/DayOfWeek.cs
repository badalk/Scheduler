using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.QuartzScheduler.Models
{
    public class DayOfWeek
    {
        public string itemId { get; set; }

        public string itemName { get; set; }

        public DaysOfWeek DayOfWeekValue { get; set; }
    }
}