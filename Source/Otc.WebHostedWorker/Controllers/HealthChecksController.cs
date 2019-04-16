using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Otc.AspNetCore.ApiBoot;
using Otc.HostedWorker.Abstractions;
using System;

namespace Otc.WebHostedWorker.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HealthChecksController : ApiController
    {
        private readonly IHostedWorkerHealth hostedWorkerHealth;

        public HealthChecksController(IHostedWorkerHealth hostedWorkerHealth)
        {
            this.hostedWorkerHealth = hostedWorkerHealth ?? throw new ArgumentNullException(nameof(hostedWorkerHealth));
        }

        [HttpGet("/healthz")]
        public IActionResult Healthz()
        {
            if (hostedWorkerHealth.Healthy)
            {
                return Ok(DateTimeOffset.Now.ToString("o"));
            }

            return StatusCode(503, $"{DateTimeOffset.Now.ToString("o")}: HostedWorker is Unhealthy.");
        }
    }
}
