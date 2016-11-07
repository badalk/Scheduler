using Sitecore.QuartzScheduler.Models;
using Sitecore.QuartzScheduler.Repository;
using Sitecore.Services.Core;
using Sitecore.Services.Infrastructure.Sitecore.Services;

namespace Sitecore.QuartzScheduler.Controllers
{
    [ServicesController]
    public class TriggerDetailController : EntityService<TriggerDetail>
    {

        public TriggerDetailController(IRepository<TriggerDetail> repository) : base(repository)
        {

        }

        public TriggerDetailController(): this(new TriggerDetailRepository())
        {

        }



    }
}