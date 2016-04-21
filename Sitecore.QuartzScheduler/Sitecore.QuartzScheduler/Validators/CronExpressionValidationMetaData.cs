using Sitecore.Services.Core.MetaData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.QuartzScheduler.Validators
{
    public class CronExpressionValidationMetaData: ValidationMetaDataBase<CronExpressionAttribute>
    {
        public CronExpressionValidationMetaData() : base("validCronExpression")
        {
        }

        public override Dictionary<string, object> Describe(ValidationAttribute attribute, string fieldName)
        {
            var metadata = base.Describe(attribute, fieldName);
            var cronExpressionAttribute = (CronExpressionAttribute) attribute;
            metadata.Add("param", @"http://www.quartz-scheduler.net/documentation/quartz-2.x/tutorial/crontrigger.html" );
            return metadata;
        }
    }
}
