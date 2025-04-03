using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Domain
{
    public class DataMetric
    {
        public string EntityKey { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
    }
}
