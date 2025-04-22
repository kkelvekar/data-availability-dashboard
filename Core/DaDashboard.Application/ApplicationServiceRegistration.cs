using DaDashboard.Application.Contracts.Application;
using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Features.Orchestrator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DaDashboard.Application
{
    /// <summary>
    /// Provides extension methods to register application-layer services with the dependency injection container.
    /// </summary>
    public static class ApplicationServiceRegistration
    {
        /// <summary>
        /// Adds application services, including the data domain orchestrator and job stats strategy factory, to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to register services with.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDataDomainOrchestrator, DataDomainOrchestrator>();          
            services.AddScoped<JobStatsStrategyFactory>();         
            return services;
        }
    }
}
