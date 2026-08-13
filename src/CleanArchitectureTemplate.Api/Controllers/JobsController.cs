using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanArchitectureTemplate.Api.Jobs;
using Hangfire.Common;

namespace CleanArchitectureTemplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // protect these endpoints
    public class JobsController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;

        public JobsController(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
        }

        [HttpPost("enqueue-heartbeat")]
        public IActionResult EnqueueHeartbeat()
        {
            _backgroundJobClient.Enqueue<SampleJobs>(job => job.HeartbeatAsync());
            return Accepted(new { Message = "Heartbeat enqueued" });
        }

        [HttpPost("recurring-heartbeat")]
        public IActionResult AddOrUpdateRecurring()
        {
            // Cron.Minutely is a method; call it to get the string expression.
            _recurringJobManager.AddOrUpdate(
                "recurring-heartbeat",
                Job.FromExpression<SampleJobs>(j => j.HeartbeatAsync()),
                Cron.Minutely(),
                new RecurringJobOptions()
            );

            return Ok(new { Message = "Recurring heartbeat scheduled" });
        }

        [HttpPost("remove-recurring")]
        public IActionResult RemoveRecurring()
        {
            _recurringJobManager.RemoveIfExists("recurring-heartbeat");
            return Ok(new { Message = "Recurring heartbeat removed" });
        }
    }
}