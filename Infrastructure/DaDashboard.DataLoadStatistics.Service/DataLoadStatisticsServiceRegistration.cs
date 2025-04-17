using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using Microsoft.Extensions.DependencyInjection;

namespace DaDashboard.DataLoadStatistics.Service
{
    public static class DataLoadStatisticsServiceRegistration
    {
        public static IServiceCollection AddDataLoadStatisticsServices(
            this IServiceCollection services,
            string activeEnvironment)
        {
            // Register a named HttpClient for the JobStats API.
            services.AddHttpClient("JobStatsClient");

            // Register the JobStatsService.
            services.AddScoped<IJobStatsService, JobStatsService>();

            // Register the strategy, passing in the active environment string
            services.AddScoped<IJobStatsStrategy>(sp =>
            {
                var jobStatsService = sp.GetRequiredService<IJobStatsService>();
                return new DataLoadStatisticServiceStrategy(jobStatsService, activeEnvironment);
            });

            return services;

        }
    }
}
