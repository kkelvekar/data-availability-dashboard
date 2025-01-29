using DaDashboard.Application.Models.Infrastructure.GraphQL;

namespace DaDashboard.DataSource.GraphQL.Models
{
    /// <summary>
    /// Matches the 'data' portion for the dataLoadMatrix query response.
    /// </summary>
    public class DataLoadMatrixResponse
    {
        public DataLoadMatrix[] dataLoadMatrix { get; set; }
    }
}
