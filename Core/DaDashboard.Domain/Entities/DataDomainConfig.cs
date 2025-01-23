using DaDashboard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Domain.Entities
{
    public class DataDomainConfig : AuditableEntity
    {
        public Guid Id { get; set; }
        public string DomainName { get; set; }
        public string SourceType { get; set; }
        public bool IsActive { get; set; }
 
        // Navigation property
        public DomainSourceTypeGraphQL DomainSourceGraphQL { get; set; }
    }
}
