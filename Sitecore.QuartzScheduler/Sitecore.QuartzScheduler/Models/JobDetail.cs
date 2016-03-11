using Quartz;
using Sitecore.Services.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Sitecore.QuartzScheduler.Models
{
    public class JobDetail : EntityIdentity
    {
 
        /// <summary>
        /// This property exists to support any SPEAK components
        /// </summary>
        public string itemId
        {
            get { return base.Id; }
        }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Type is mandatory. Please enter type of job in fully qualified .NET type format.")]
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Group is mandatory")]
        public string Group { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Job Key is mandatory. Please enter Job key that will be used to identify the job definition.")]
        public string JobKey { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [StringLength(maximumLength: 100, ErrorMessage = "Description should not be more than 100 characters", MinimumLength = 0)]
        public string Description { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string JobData { get; set; }


    }
}
