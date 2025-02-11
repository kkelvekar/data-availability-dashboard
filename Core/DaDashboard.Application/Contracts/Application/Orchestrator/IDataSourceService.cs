using DaDashboard.Domain;
using DaDashboard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Contracts.Application.Orchestrator
{
    public interface IDataSourceService
    {
        // The source type this implementation handles (e.g., "GraphQL", "REST", "SQL", etc.)
        string SourceType { get; }

        // Returns the DataMetric for the given domain configuration and effective date.
        Task<List<DataMetric>> GetDataMetricAsync(DataDomainConfig config, DateTime? effectiveDate);
    }
}
