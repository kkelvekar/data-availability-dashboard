using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Domain.Entities
{
    public class DomainSourceTypeGraphQL : AuditableEntity
    {
        public Guid DataDomainId { get; set; }
        public string DevBaseUrl { get; set; }
        public string QaBaseUrl { get; set; }
        public string PreProdBaseUrl { get; set; }
        public string ProdBaseUrl { get; set; }
        public string EndpointPath { get; set; }
        public string EntityKey { get; set; }

        // Navigation property
        public DataDomainConfig DataDomainConfig { get; set; }
    }
}
