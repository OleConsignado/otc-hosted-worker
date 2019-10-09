using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Otc.AspNetCore.ApiBoot;
using Otc.HostedWorker.Abstractions;

namespace Otc.WebHostedWorker.Controllers
{
    [AllowAnonymous]
    public class TriggerController : ApiController
    {
        private readonly IHostedWorkerTrigger hostedWorkerTrigger;

        public TriggerController(IHostedWorkerTrigger hostedWorkerTrigger)
        {
            this.hostedWorkerTrigger = hostedWorkerTrigger ??
                throw new System.ArgumentNullException(nameof(hostedWorkerTrigger));
        }

        [HttpPost("Pull")]
        public void Pull()
        {
            hostedWorkerTrigger.Pull();
        }
    }
}
