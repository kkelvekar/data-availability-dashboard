using DaDashboard.Application.Models.Infrastructure.GraphQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Contracts.Infrastructure.GraphQL
{
    public interface IGraphQLDomainMetricsService
    {
        public Task GetDomainDetails(GraphQLServiceConfig qLServiceConfig);
    }
}
