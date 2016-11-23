using Quartz;
using Sitecore.QuartzScheduler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Sitecore.QuartzScheduler.Validators
{
    [AttributeUsage(AttributeTargets.Property |   AttributeTargets.Field, AllowMultiple = false)]
    public class CronExpressionAttribute : ValidationAttribute
    {
        //public override bool IsValid(object value)
        //{
        //    return CronExpression.IsValidExpression(value.ToString());
        //}

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isValid = true;
            if (value != null)
            {
                if (((TriggerDetail) validationContext.ObjectInstance).ScheduleTypeValue.ToLower() == "custom")
                    isValid = CronExpression.IsValidExpression(value.ToString());
            }

            return isValid ? ValidationResult.Success : new ValidationResult(ErrorMessage);
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture,
              ErrorMessageString, name);
        }
    }
}