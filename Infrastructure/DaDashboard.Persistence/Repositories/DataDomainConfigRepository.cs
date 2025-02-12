using DaDashboard.Application.Contracts.Persistence;
using DaDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
        public async Task<List<DataDomainConfig>> GetAll(bool isActive)
        {
            return await _dbContext.DataDomainConfigs
                 .Include(d => d.DomainSourceGraphQL)  // Eager load the child entity
                 .Where(d => d.IsActive == isActive)   // Filter by active status
                 .AsNoTracking()                       // Good for read-only operations
                 .ToListAsync();
        }
    }
}

//public record DataDomainConfigFlat
//{
//    // DataDomainConfig properties (parent)
//    public Guid Id { get; init; }
//    public string DomainName { get; init; }
//    public string SourceType { get; init; }
//    public bool IsActive { get; init; }
//    public DateTime CreatedDate { get; init; }
//    public DateTime UpdatedDate { get; init; }

//    // DomainSourceTypeGraphQL properties (child) prefixed with "GraphQL"
//    public Guid? GraphQLId { get; init; }
//    public Guid? GraphQLDataDomainId { get; init; }
//    public string GraphQLDevBaseUrl { get; init; }
//    public string GraphQLQaBaseUrl { get; init; }
//    public string GraphQLPreProdBaseUrl { get; init; }
//    public string GraphQLProdBaseUrl { get; init; }
//    public string GraphQLEndpointPath { get; init; }
//    public string GraphQLMetadata { get; init; }
//    public DateTime? GraphQLCreatedDate { get; init; }
//    public DateTime? GraphQLUpdatedDate { get; init; }
//}



//public DataDomainConfig MapFlatToEntity(DataDomainConfigFlat flat)
//{
//    var config = new DataDomainConfig
//    {
//        Id = flat.Id,
//        DomainName = flat.DomainName,
//        SourceType = flat.SourceType,
//        IsActive = flat.IsActive,
//        CreatedDate = flat.CreatedDate,
//        UpdatedDate = flat.UpdatedDate
//    };

//    // Only map the child entity if GraphQLId is present.
//    if (flat.GraphQLId.HasValue)
//    {
//        var child = new DomainSourceTypeGraphQL
//        {
//            Id = flat.GraphQLId.Value,
//            DataDomainId = flat.GraphQLDataDomainId ?? flat.Id,
//            DevBaseUrl = flat.GraphQLDevBaseUrl,
//            QaBaseUrl = flat.GraphQLQaBaseUrl,
//            PreProdBaseUrl = flat.GraphQLPreProdBaseUrl,
//            ProdBaseUrl = flat.GraphQLProdBaseUrl,
//            EndpointPath = flat.GraphQLEndpointPath,
//            Metadata = flat.GraphQLMetadata,
//            CreatedDate = flat.GraphQLCreatedDate ?? DateTime.MinValue,
//            UpdatedDate = flat.GraphQLUpdatedDate ?? DateTime.MinValue
//        };

//        // Set the navigation property back to the parent.
//        child.DataDomainConfig = config;
//        config.DomainSourceGraphQL = child;
//    }

//    return config;
//}


//// Retrieve flat data from your DataFacade (example query and parameter)
//const string query = @"
//SELECT 
//    d.Id, 
//    d.DomainName, 
//    d.SourceType, 
//    d.IsActive, 
//    d.CreatedDate, 
//    d.UpdatedDate, 
//    g.Id AS GraphQLId,
//    g.DataDomainId AS GraphQLDataDomainId, 
//    g.DevBaseUrl AS GraphQLDevBaseUrl, 
//    g.QaBaseUrl AS GraphQLQaBaseUrl, 
//    g.PreProdBaseUrl AS GraphQLPreProdBaseUrl, 
//    g.ProdBaseUrl AS GraphQLProdBaseUrl, 
//    g.EndpointPath AS GraphQLEndpointPath, 
//    g.Metadata AS GraphQLMetadata,
//    g.CreatedDate AS GraphQLCreatedDate,
//    g.UpdatedDate AS GraphQLUpdatedDate
//FROM DataDomainConfig d 
//LEFT JOIN DomainSourceTypeGraphQL g 
//    ON d.Id = g.DataDomainId 
//WHERE d.IsActive = @isActive";


//// Map flat records into your hierarchical entities.
//var flatList = Datafacade.GetObjectList<DataDomainConfigFlat>(
//    query,
//    ("isActive", true)
//);

//// Map flat records into hierarchical entities.
//var hierarchicalList = flatList.Select(flat => MapFlatToEntity(flat)).ToList();

