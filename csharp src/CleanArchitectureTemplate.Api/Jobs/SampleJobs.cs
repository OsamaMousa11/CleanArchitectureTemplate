using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureTemplate.Api.Jobs
{
    public class SampleJobs
    {
        private readonly ILogger<SampleJobs> _logger;

        public SampleJobs(ILogger<SampleJobs> logger)
        {
            _logger = logger;
        }

        public Task HeartbeatAsync()
        {
            _logger.LogInformation("Heartbeat job executed at {Now}", DateTime.UtcNow);
            return Task.CompletedTask;
        }
    }
}