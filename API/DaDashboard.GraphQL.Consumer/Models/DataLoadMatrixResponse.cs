using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.GraphQL.Consumer.Models
{
    public class DataLoadMatrixResponse
    {
        public DataLoadMatrix[] dataLoadMatrix { get; set; }
    }

    public class DataLoadMatrix
    {
        public int count { get; set; }
        public DateTime effectiveDate { get; set; }
    }
}
