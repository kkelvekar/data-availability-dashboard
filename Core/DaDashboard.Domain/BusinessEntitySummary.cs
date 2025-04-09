using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Domain
{
    public class EntityStatus
    {
        public RagIndicator Indicator { get; set; }
        public string Description { get; set; }
    }

    public enum RagIndicator
    {
        Red,
        Amber,
        Green
    }

    public class BusinessEntitySummary
    {
        public Guid Id { get; set; }
        public string ApplicationOwner { get; set; } = null!;
        public string BusinessEntity { get; set; } = null!;
        public DateTime LatestLoadDate { get; set; }
        public int TotalRecordsLoaded { get; set; }
        public IEnumerable<string> DependentFuncs { get; set; } = new List<string>();
        public EntityStatus Status { get; set; } = new EntityStatus();
    }
}
