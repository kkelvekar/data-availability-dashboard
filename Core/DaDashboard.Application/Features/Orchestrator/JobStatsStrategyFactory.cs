// JobStatsStrategyFactory.cs
using System;
using System.Collections.Generic;
using System.Linq;
using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Domain.Entities;

namespace DaDashboard.Application.Features.Orchestrator
{
    public class JobStatsStrategyFactory
    {
        private readonly IEnumerable<IJobStatsStrategy> _strategies;

        // DI will give us *all* IJobStatsStrategy implementations
        public JobStatsStrategyFactory(IEnumerable<IJobStatsStrategy> strategies)
        {
            _strategies = strategies;
        }

        /// <summary>
        /// Looks up the strategy instance by its Name property.
        /// </summary>
        public IJobStatsStrategy GetStrategy(string strategyName)
        {
            var strat = _strategies
                .FirstOrDefault(s => s.Name.Equals(strategyName, StringComparison.OrdinalIgnoreCase));

            if (strat == null)
                throw new InvalidOperationException($"No IJobStatsStrategy with Name='{strategyName}' is registered.");

            return strat;
        }
    }
}
