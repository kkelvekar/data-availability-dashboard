using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.DataLoadStatistics.Service
{
    public static class DataLoadStatisticsServiceRegistration
    {
        public static IServiceCollection AddDataLoadStatisticsServices(this IServiceCollection services)
        {
            // Register a named HttpClient for the JobStats API.
            services.AddHttpClient("JobStatsClient", client =>
            {
                // Replace with your actual base address for the JobStats API.
                client.BaseAddress = new Uri("https://localhost:7080/");
            });

            // Register the JobStatsService.
            services.AddScoped<IJobStatsService, JobStatsService>();

            return services;
        }
    }
}
