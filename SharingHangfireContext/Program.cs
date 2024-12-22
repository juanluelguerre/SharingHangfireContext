using Hangfire;
using Microsoft.EntityFrameworkCore;
using SharingHangfireContext.Accessors;
using SharingHangfireContext.Providers;
using SharingHangfireContext.Services;

namespace SharingHangfireContext;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor()
            .AddDbContext<NotesDbContext>(options => options.UseInMemoryDatabase("NotesDb"))
            .AddScoped<NotesCleanupService>()
            .AddScoped<NotesService>()
            .AddScoped<IDataSeedProvider, DataSeedProvider>()
            .AddHangfire(config => config.UseInMemoryStorage())
            .AddHangfireServer(options => { options.Queues = ["default"]; })
            .AddSwaggerGen()
            .AddControllers();

        services.AddScoped<ContextDataProvider>();
        // It's registered because it's used to set the context data in the Hangfire job
        services.AddScoped<IContextDataProvider>(
            x => x.GetRequiredService<ContextDataProvider>());

        // It's required to be resolved when IContextAccessor is requested
        services.AddScoped<ContextAccessor>();

        services.AddScoped<IContextAccessor>(
            sp =>
            {
                var httpContext = sp.GetService<IHttpContextAccessor>()?.HttpContext;
                if (httpContext != null)
                    return sp.GetRequiredService<ContextAccessor>();

                return sp.GetRequiredService<IContextDataProvider>();
            });
    }

    public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var dataSeedProvider = scope.ServiceProvider.GetRequiredService<IDataSeedProvider>();
            dataSeedProvider.Seed().Wait();
        }
        
        app.UseRouting();
        app.UseHangfireDashboard();
        app.UseSwagger();

        app.UseSwaggerUI(
            c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = String.Empty; // Set Swagger UI at the app's root
            });

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
