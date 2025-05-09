namespace DataLoadStatistics.API.DTO
{
    // DTO for filtering JobStats using query parameters.
    public class JobStatsFilterRequest
    {
        public List<string> BusinessEntities { get; set; } = new List<string>();
        public DateTime? ReferenceDate { get; set; }
    }
}
