using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Application.Contracts.Infrastructure.GraphQL;
using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DaDashboardDbContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("DaDashboardDb")), ServiceLifetime.Transient);

            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IDataDomainConfigRepository, DataDomainConfigRepository>();
            return services;
        }
    }
}
