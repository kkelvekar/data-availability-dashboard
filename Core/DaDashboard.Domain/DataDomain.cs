namespace DaDashboard.Domain
{
    public class DataDomain
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<DataMetric> Metrics { get; set; }
    }
}
