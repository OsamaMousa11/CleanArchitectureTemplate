using CleanArchitectureTemplate.Api.Extensions;
using CleanArchitectureTemplate.Api.Hubs;
using CleanArchitectureTemplate_Api.Middlewares;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
using CleanArchitectureTemplate.Api.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.ServiceConfiguration(builder.Configuration);
builder.Services.AddSignalR();

// Register job types for DI so Hangfire and controllers can resolve them
builder.Services.AddTransient<SampleJobs>();

builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
});

// --- Hangfire configuration (uses the same SQL connection string) ---
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("connstr"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

builder.Services.AddHangfireServer();
// -------------------------------------------------------------------

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CleanArchitectureTemplate_infrastructure.Data.AppDbContext>();
        await context.Database.MigrateAsync();

        await CleanArchitectureTemplate_infrastructure.Persistence.DbSeeder.SeedAdminUserAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CleanArchitectureTemplate API v1");
    options.RoutePrefix = "swagger";
});

app.UseExceptionHandling();
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ✅ Map SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notifications");
//app.MapHub<MessageHub>("/hubs/messages");

// --- Hangfire dashboard (unsecured by default) ---
app.UseHangfireDashboard("/hangfire");
// Example recurring job (optional): runs every minute
RecurringJob.AddOrUpdate("heartbeat", () => Console.WriteLine("Hangfire heartbeat: " + DateTime.UtcNow), Cron.Minutely);
// -------------------------------------------------------------------

app.Run();