using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Models.Infrastructure.DataLoadStatistics
{
    public class JobStatsRequest
    {
        public List<string> BusinessEntities { get; set; } = new List<string>();
        public DateTime? RecordAsOfDate { get; set; }
    }
}
