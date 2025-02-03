namespace DaDashboard.Domain
{
    public class DataDomain
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DataMetric Metric { get; set; }
    }
}
