namespace DaDashboard.API.DTO
{
    public class BusinessEntitySummaryResponse
    {
        public Guid Id { get; set; }
        public string ApplicationOwner { get; set; }
        public string BusinessEntity { get; set; }
        public DateTime LatestLoadDate { get; set; }
        public int TotalRecordsLoaded { get; set; }
        public IEnumerable<string> DependentFuncs { get; set; }
        public EntityStatus Status { get; set; }
    }

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
}
