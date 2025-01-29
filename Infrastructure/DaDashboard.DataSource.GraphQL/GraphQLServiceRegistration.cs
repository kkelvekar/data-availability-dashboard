using DaDashboard.Application.Contracts.Infrastructure.GraphQL;
using DaDashboard.DataSource.GraphQL.Common;
using DaDashboard.DataSource.GraphQL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DaDashboard.DataSource.GraphQL
{
    public static class GraphQLServiceRegistration
    {
        public static IServiceCollection AddGraphQLService(this IServiceCollection services)
        {
            services.AddScoped<GraphQLClientService>();
            services.AddScoped<IGraphQLDomainMetricsService, GraphQLDomainMetricsService>();
            return services;
        }
    }
}
