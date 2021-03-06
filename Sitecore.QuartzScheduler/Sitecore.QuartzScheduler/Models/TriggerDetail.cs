﻿using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.QuartzScheduler.Validators;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.QuartzScheduler.Models
{
    public class TriggerDetail : EntityIdentity
    {
        /// <summary>
        /// To support SPEAK components
        /// </summary>
        public string itemId
        {
            get { return base.Id; }
            set { base.Id = value;  }
        }

        public string ParentItemId { get; set; }

        private string _scheduleTypeValue;
        /// <summary>
        /// User friendly name to identify the trigger name. For example: EveryHrReportingSchedule
        /// </summary>
        [Required(ErrorMessage = "Trigger Key is required")]
        public string TriggerKey { get; set; }

        public int Priority { get; set; }

        /// <summary>
        /// At what time the schedule should start
        /// </summary>
        [Required(ErrorMessage = "Start Date Time is required")]
        public DateTime StartTime {
            get;
            set;
        }

        /// <summary>
        /// At what time scheduler should stop executing the trigger. This is useful if you want to run a job only between specified duration.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// To indicate if this is Daily, Weekly Or Monthly schedule
        /// </summary>
        [Required(ErrorMessage = "Schedule Type is required. Please select valid Schedule Type.")]
        public string ScheduleType { get; set; }

        public string ScheduleTypeValue
        {
            get
            {
                if (string.IsNullOrEmpty(_scheduleTypeValue))
                {
                    Database masterDb = Factory.GetDatabase("master");
                    _scheduleTypeValue = masterDb.GetItem(new ID(this.ScheduleType)).Name;
                }

                return _scheduleTypeValue;
            }
        }

        /// <summary>
        /// Days Of the Week when this schedule should be applied if Schedule Type is Weekly (e.g. To indicate that this Job Runs only on Sat & Sunday)
        /// </summary>
        private List<DayOfWeek> _daysOfWeek;
        public List<DayOfWeek> DaysOfWeeks
        {
            get
            {
                if (_daysOfWeek == null)
                    _daysOfWeek = new List<DayOfWeek>();
                
                return _daysOfWeek;
            }
            set { _daysOfWeek = value; }
        }

        /// <summary>
        /// If schedule Type is Monthly then on which day of the month should this schedule run. Keep February Month in mind
        /// </summary>
        /// <seealso cref="SchedulerType"/>
        public int DayOfMonth { get; set; }


        /// <summary>
        /// Number of times you want to repeat triggering this Job.
        /// </summary>
        public int RepeatCount{ get; set; }

        /// <summary>
        /// Frequency at which this shcedule should repeat based on the #ref: RepeatFrequencyUnit
        /// </summary>
        /// <seealso cref="RepeatFrequncyUnit"/>
        public int RepeatInterval { get; set; }

        /// <summary>
        /// A custom cron expression instead of UI based schedule. Cron expression can be defined based on Quartz Cron expression guidelines.
        /// </summary>
        [CronExpression(ErrorMessage = "Not a valid Cron Expression.")]

        public string CronExpression { get; set; }

    }


}
