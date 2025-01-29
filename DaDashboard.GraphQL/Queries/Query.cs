using DaDashboard.GraphQL.Models;

namespace DaDashboard.GraphQL.Queries
{
    // This represents the "root" Query in the GraphQL schema.
    public class Query
    {
        // dataLoadMatrix(entityName: "BENCHMARK", effectiveDate: null) {...}
        // Return a list of DataLoadMatrix objects.
        public IEnumerable<DataLoadMatrix> GetDataLoadMatrix(string entityName, DateTime? effectiveDate)
        {
            // For a real implementation, you might fetch from a database
            // or call an external service. Here we just return hard-coded data.
            var sampleData = new List<DataLoadMatrix>
        {
            new DataLoadMatrix { Count = 113, EffectiveDate = new DateTime(2025, 1, 27) },
            new DataLoadMatrix { Count = 100, EffectiveDate = new DateTime(2025, 1, 29) },
            new DataLoadMatrix { Count = 103, EffectiveDate = new DateTime(2025, 1, 25) },
            // etc...
        };

            // Optionally filter based on entityName and effectiveDate if needed
            // For example:
            if (!string.IsNullOrEmpty(entityName))
            {
                // some filter logic on sampleData
            }

            // Filter by date if provided
            if (effectiveDate.HasValue)
            {
                // Compare only the date part (ignoring time), if that's your requirement:
                var targetDate = effectiveDate.Value.Date;
                sampleData = sampleData
                    .Where(x => x.EffectiveDate.Date == targetDate)
                    .ToList();
            }

            return sampleData;
        }
    }

}
