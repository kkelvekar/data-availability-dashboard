using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Domain
{
    public class BusinessEntitySummary
    {
        public Guid BusinessEntityID { get; set; }
        public string ApplicationOwner { get; set; }
        public string BusinessEntity { get; set; }
        public DateTime LatestLoadDate { get; set; }
        public int TotalRecordsLoaded { get; set; }
    }
}
