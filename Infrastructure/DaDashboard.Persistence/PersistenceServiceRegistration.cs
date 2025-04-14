using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DaDashboard.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DaDashboardDbContext>(options =>
               options.UseSqlServer(configuration.GetConnectionString("DaDashboardDb")), ServiceLifetime.Transient);

            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));
           
            return services;
        }
    }
}
