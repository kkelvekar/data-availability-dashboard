using DaDashboard.GraphQL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaDashboard.GraphQL.Queries
{
    // This represents the "root" Query in the GraphQL schema.
    public class Query
    {
        // dataLoadMatrix(entityName: "BENCHMARK", effectiveDate: null) {...}
        // Return a list of DataLoadMatrix objects.
        public IEnumerable<DataLoadMatrix> GetDataLoadMatrix(string entityName, DateTime? effectiveDate)
        {
            List<DataLoadMatrix> sampleData;

            // Choose sample data based on the entityName.
            switch (entityName?.ToUpperInvariant())
            {
                case "BENCHMARK":
                    sampleData = new List<DataLoadMatrix>
                    {
                        new DataLoadMatrix { Count = 113, EffectiveDate = new DateTime(2025, 1, 27) },
                        new DataLoadMatrix { Count = 100, EffectiveDate = new DateTime(2025, 1, 29) },
                        new DataLoadMatrix { Count = 103, EffectiveDate = new DateTime(2025, 1, 25) }
                    };
                    break;

                case "FXRATE":
                    sampleData = new List<DataLoadMatrix>
                    {
                        new DataLoadMatrix { Count = 90, EffectiveDate = new DateTime(2025, 1, 27) },
                        new DataLoadMatrix { Count = 85, EffectiveDate = new DateTime(2025, 1, 29) },
                        new DataLoadMatrix { Count = 88, EffectiveDate = new DateTime(2025, 1, 25) }
                    };
                    break;

                default:
                    // Default sample data if entityName is not provided or does not match known values.
                    sampleData = new List<DataLoadMatrix>();
                    break;
            }

            // If effectiveDate is provided, filter the sample data based on that date.
            if (effectiveDate.HasValue)
            {
                var targetDate = effectiveDate.Value.Date;
                sampleData = sampleData.Where(x => x.EffectiveDate.Date == targetDate).ToList();
            }

            return sampleData;
        }
    }
}
