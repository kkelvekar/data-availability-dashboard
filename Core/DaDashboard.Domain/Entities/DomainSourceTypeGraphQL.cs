using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Domain.Entities
{
    public class DomainSourceTypeGraphQL : AuditableEntity
    {
        public Guid Id { get; set; }  // New primary key
        public Guid DataDomainId { get; set; }
        public string DevBaseUrl { get; set; }
        public string QaBaseUrl { get; set; }
        public string PreProdBaseUrl { get; set; }
        public string ProdBaseUrl { get; set; }
        public string EndpointPath { get; set; }
        public string Metadata { get; set; }

        // Navigation property
        public DataDomainConfig DataDomainConfig { get; set; }
    }
}
