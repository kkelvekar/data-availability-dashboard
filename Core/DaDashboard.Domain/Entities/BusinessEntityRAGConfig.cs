using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Domain.Entities
{
    public class BusinessEntityRAGConfig : AuditableEntity
    {
        public Guid Id { get; set; }
        public string RedExpression { get; set; }
        public string AmberExpression { get; set; }
        public string GreenExpression { get; set; }
        public string Description { get; set; }
    }
}
