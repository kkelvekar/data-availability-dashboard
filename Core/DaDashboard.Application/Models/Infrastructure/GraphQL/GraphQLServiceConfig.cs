using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Models.Infrastructure.GraphQL
{
    public class GraphQLServiceConfig
    {
        public string Domain { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string EndpointPath { get; set; } = string.Empty;
        public string EntityKey { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
