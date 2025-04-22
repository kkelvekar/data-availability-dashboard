// JobStatsStrategyFactory.cs
using System;
using System.Collections.Generic;
using System.Linq;
using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Domain.Entities;

namespace DaDashboard.Application.Features.Orchestrator
{
    /// <summary>
    /// Factory responsible for resolving <see cref="IJobStatsStrategy"/> implementations by strategy name.
    /// </summary>
    public class JobStatsStrategyFactory
    {
        private readonly IEnumerable<IJobStatsStrategy> _strategies;

        // DI will give us *all* IJobStatsStrategy implementations
        /// <summary>
        /// Initializes a new instance of the <see cref="JobStatsStrategyFactory"/> class with the provided strategies.
        /// </summary>
        /// <param name="strategies">All available implementations of <see cref="IJobStatsStrategy"/> injected via DI.</param>
        public JobStatsStrategyFactory(IEnumerable<IJobStatsStrategy> strategies)
        {
            _strategies = strategies;
        }

        /// <summary>
        /// Retrieves the registered job stats strategy matching the specified name (case-insensitive).
        /// </summary>
        /// <param name="strategyName">The name identifying the desired strategy.</param>
        /// <returns>The matching <see cref="IJobStatsStrategy"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no matching strategy is registered.</exception>
        public IJobStatsStrategy GetStrategy(string strategyName)
        {
            var strat = _strategies
                .FirstOrDefault(s => s.StrategyName.Equals(strategyName, StringComparison.OrdinalIgnoreCase));

            if (strat == null)
                throw new InvalidOperationException($"No IJobStatsStrategy with Name='{strategyName}' is registered.");

            return strat;
        }
    }
}
