using System;
using System.Collections.Generic;
using System.Linq;
using DaDashboard.Application.Contracts.Application.Orchestrator;
using DaDashboard.Domain.Entities;
using DaDashboard.Domain;
using DynamicExpresso;
using AppJobStats = DaDashboard.Application.Models.Infrastructure.DataLoadStatistics.JobStats;

namespace DaDashboard.Application.Features.Orchestrator
{
    /// <summary>
    /// Evaluates RAG status by executing dynamic expressions defined in BusinessEntityRAGConfig.
    /// </summary>
    public class RagStatusEvaluator : IRagStatusEvaluator
    {
        private readonly Interpreter _interpreter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RagStatusEvaluator"/> class,
        /// configuring the interpreter with necessary references.
        /// </summary>
        public RagStatusEvaluator()
        {
            var options = InterpreterOptions.Default | InterpreterOptions.LambdaExpressions;
            _interpreter = new Interpreter(options)
                .Reference(typeof(Enumerable))
                .Reference(typeof(DateTime))
                .Reference(typeof(AppJobStats));
        }

        /// <inheritdoc/>
        public EntityStatus Evaluate(IEnumerable<AppJobStats> jobStats, BusinessEntityRAGConfig ragConfig, DateTime referenceDate)
        {
            if (ragConfig == null) throw new ArgumentNullException(nameof(ragConfig));
            if (jobStats == null) throw new ArgumentNullException(nameof(jobStats));

            // 1) Check Red
            var redRule = _interpreter.ParseAsDelegate<Func<IEnumerable<AppJobStats>, DateTime, bool>>(
                ragConfig.RedExpression, "jobStats", "currentDate");
            if (redRule(jobStats, referenceDate))
            {
                return new EntityStatus { Indicator = RagIndicator.Red, Description = RagIndicator.Red.ToString() };
            }

            // 2) Check Amber
            var amberRule = _interpreter.ParseAsDelegate<Func<IEnumerable<AppJobStats>, DateTime, bool>>(
                ragConfig.AmberExpression, "jobStats", "currentDate");
            if (amberRule(jobStats, referenceDate))
            {
                return new EntityStatus { Indicator = RagIndicator.Amber, Description = RagIndicator.Amber.ToString() };
            }

            // 3) Check Green
            var greenRule = _interpreter.ParseAsDelegate<Func<IEnumerable<AppJobStats>, DateTime, bool>>(
                ragConfig.GreenExpression, "jobStats", "currentDate");
            if (greenRule(jobStats, referenceDate))
            {
                return new EntityStatus { Indicator = RagIndicator.Green, Description = RagIndicator.Green.ToString() };
            }

            // Fallback to Green if no rules matched
            return new EntityStatus { Indicator = RagIndicator.Green, Description = RagIndicator.Green.ToString() };
        }
    }
}