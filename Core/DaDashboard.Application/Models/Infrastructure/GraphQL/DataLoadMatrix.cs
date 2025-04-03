namespace DaDashboard.Application.Models.Infrastructure.GraphQL
{
    /// <summary>
    /// Matches each record in dataLoadMatrix.
    /// </summary>
    public class DataLoadMatrix
    {
        public int count { get; set; }
        public DateTime effectiveDate { get; set; }
        public string entityKey { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
