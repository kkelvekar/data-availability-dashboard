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

   

}
