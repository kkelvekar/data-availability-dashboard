using DaDashboard.Application.Contracts.Application;
using DaDashboard.Application.Features.Orchestrator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DaDashboard.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDataDomainOrchestrator, DataDomainOrchestrator>();
            return services;
        }
    }
}
