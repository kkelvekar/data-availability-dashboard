using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Domain.Entities
{
    public class BusinessEntity : AuditableEntity
    {
        public Guid Id { get; set; }
        public string ApplicationOwner { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string DependentFunctionalities { get; set; }

        // Foreign key for BusinessEntityConfig (DataDomainSourceConfig)
        public Guid BusinessEntityConfigId { get; set; }

        // Foreign key for BusinessEntityRAGConfig
        public Guid BusinessEntityRAGConfigId { get; set; }

        public bool IsActive { get; set; }

        // Navigation properties
        public BusinessEntityConfig BusinessEntityConfig { get; set; }
        public BusinessEntityRAGConfig BusinessEntityRAGConfig { get; set; }
    }
}
