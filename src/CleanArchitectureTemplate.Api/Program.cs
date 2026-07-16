using CleanArchitectureTemplate.Api.Extensions;
using CleanArchitectureTemplate_Api.Middlewares;
using Microsoft.EntityFrameworkCore; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.ServiceConfiguration(builder.Configuration);
builder.Services.AddSignalR();

builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
});

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
app.Run();