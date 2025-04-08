namespace DaDashboard.API.DTO
{
    public class DataDomainResponse
    {
        public Guid Id { get; set; }
        public string DomainName { get; set; }
        public string BusinessEntity { get; set; }
        public int Count { get; set; }
        public DateTime LoadDate { get; set; }
    }

    public enum RagStatusEnum
    {
        Red,
        Amber,
        Green
    }

    public class BusinessEntitySummary
    {
        public Guid Id { get; set; }
        public string ApplicationOwner { get; set; }
        public string BusinessEntity { get; set; }
        public DateTime LatestLoadDate { get; set; }
        public int TotalRecordsLoaded { get; set; }
        public IEnumerable<string> DependentFuncs { get; set; }
        public RagStatusEnum RagStatus { get; set; }
    }

}
