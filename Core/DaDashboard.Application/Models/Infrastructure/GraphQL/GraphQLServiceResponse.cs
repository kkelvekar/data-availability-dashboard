using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Models.Infrastructure.GraphQL
{
    public class GraphQLServiceResponse
    {
        public string Domain { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime Date { get; set; }
    }
}
