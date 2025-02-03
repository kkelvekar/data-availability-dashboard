using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Persistence.Repositories
{
    public class DataDomainConfigRepository : BaseRepository<DataDomainConfig>, IDataDomainConfigRepository
    {
        public DataDomainConfigRepository(DaDashboardDbContext dbContext) : base(dbContext)
        {
        }
        public Task<List<DataDomainConfig>> GetAll(bool isActive)
        {
            // Create mock data for two domains: BENCHMARK and FXRATE.
            var benchmarkConfig = new DataDomainConfig
            {
                Id = Guid.NewGuid(),
                DomainName = "BENCHMARK",
                SourceType = "GraphQL",
                IsActive = isActive,
                // You could also set CreatedDate / UpdatedDate if needed.
                DomainSourceGraphQL = new DomainSourceTypeGraphQL
                {
                    // DataDomainId will be set to the parent config's Id below.
                    DevBaseUrl = "https://localhost:7204",
                    QaBaseUrl = "https://localhost:7201",
                    PreProdBaseUrl = "https://localhost:7202",
                    ProdBaseUrl = "https://localhost:7204", // As used in the real call example.
                    EndpointPath = "graphql",
                    EntityKey = "BENCHMARK"
                }
            };

            var fxrateConfig = new DataDomainConfig
            {
                Id = Guid.NewGuid(),
                DomainName = "FXRATE",
                SourceType = "GraphQL",
                IsActive = isActive,
                DomainSourceGraphQL = new DomainSourceTypeGraphQL
                {
                    DevBaseUrl = "https://localhost:7204",
                    QaBaseUrl = "https://localhost:7201",
                    PreProdBaseUrl = "https://localhost:7202",
                    ProdBaseUrl = "https://localhost:7204",
                    EndpointPath = "graphql",
                    EntityKey = "FXRATE"
                }
            };

            // Ensure the child configuration knows which parent it belongs to.
            benchmarkConfig.DomainSourceGraphQL.DataDomainId = benchmarkConfig.Id;
            fxrateConfig.DomainSourceGraphQL.DataDomainId = fxrateConfig.Id;

            var mockData = new List<DataDomainConfig> { benchmarkConfig, fxrateConfig };

            return Task.FromResult(mockData);
        }
    }
}

