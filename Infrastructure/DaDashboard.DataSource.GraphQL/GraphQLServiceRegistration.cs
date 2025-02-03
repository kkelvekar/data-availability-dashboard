using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Contracts.Infrastructure.GraphQL;
using DaDashboard.DataSource.GraphQL.Helpers;
using DaDashboard.DataSource.GraphQL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DaDashboard.DataSource.GraphQL
{
    public static class GraphQLServiceRegistration
    {
        public static IServiceCollection AddGraphQLServices(this IServiceCollection services)
        {
            services.AddScoped<GraphQLClientService>();
            services.AddScoped<IGraphQLDomainMetricsService, GraphQLDomainMetricsService>();
            services.AddScoped<IDataSourceService, GraphQLDataSourceService>();
            return services;
        }
    }
}
