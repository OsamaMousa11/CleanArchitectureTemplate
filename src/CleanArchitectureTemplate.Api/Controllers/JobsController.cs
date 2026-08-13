using System.Diagnostics;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanArchitectureTemplate.Api.Jobs;
using CleanArchitectureTemplate_Application.ServiceContract;

namespace CleanArchitectureTemplate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly SampleJobs _sampleJobs;
        private readonly IMailingService _mailingService;

        // SINGLE constructor — remove any other constructors to avoid DI ambiguity
        public JobsController(
            IBackgroundJobClient backgroundJobClient,
            IRecurringJobManager recurringJobManager,
            SampleJobs sampleJobs,
            IMailingService mailingService)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
            _sampleJobs = sampleJobs;
            _mailingService = mailingService;
        }

        // 1) Use Hangfire (background) - returns immediately with job id
        [HttpPost("enqueue-heartbeat")]
        public IActionResult EnqueueHeartbeat()
        {
            var jobId = _backgroundJobClient.Enqueue<SampleJobs>(job => job.HeartbeatAsync());
            return Accepted(new { Message = "Heartbeat enqueued", JobId = jobId });
        }

        // 2) Run immediately (no Hangfire) - caller waits for completion
        [HttpPost("run-immediate")]
        public async Task<IActionResult> RunImmediate()
        {
            var sw = Stopwatch.StartNew();
            await _sampleJobs.HeartbeatAsync();
            sw.Stop();

            return Ok(new
            {
                Message = "Heartbeat executed immediately",
                ElapsedMilliseconds = sw.ElapsedMilliseconds
            });
        }

        // Schedule recurring job (Hangfire)
        [HttpPost("recurring-heartbeat")]
        public IActionResult AddOrUpdateRecurring()
        {
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

        // Request DTO for sending email
        public record EmailRequest(string To, string Subject, string Body);

        // Endpoint A: send email using Hangfire (background)
        [HttpPost("send-email-hangfire")]
        public IActionResult SendEmailHangfire([FromBody] EmailRequest request)
        {
            // Enqueue by interface so DI resolves the implementation at runtime
            var jobId = _backgroundJobClient.Enqueue<IMailingService>(svc => svc.SendMessageAsync(request.To, request.Subject, request.Body, null));
            return Accepted(new { Message = "Email enqueued", JobId = jobId });
        }

        // Endpoint B: send email immediately (no Hangfire) — caller waits
        [HttpPost("send-email-immediate")]
        public async Task<IActionResult> SendEmailImmediate([FromBody] EmailRequest request)
        {
            var sw = Stopwatch.StartNew();
            await _mailingService.SendMessageAsync(request.To, request.Subject, request.Body, null);
            sw.Stop();

            return Ok(new
            {
                Message = "Email sent immediately",
                ElapsedMilliseconds = sw.ElapsedMilliseconds
            });
        }
    }
}