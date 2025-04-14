using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Domain.Entities
{
    public class BusinessEntityConfig : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Metadata { get; set; }
    }
}
