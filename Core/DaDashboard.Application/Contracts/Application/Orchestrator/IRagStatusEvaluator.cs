using System;
using System.Collections.Generic;
using DaDashboard.Domain.Entities;
using DaDashboard.Domain;
using AppJobStats = DaDashboard.Application.Models.Infrastructure.DataLoadStatistics.JobStats;

namespace DaDashboard.Application.Contracts.Application.Orchestrator
{
    /// <summary>
    /// Defines functionality for evaluating RAG (Red, Amber, Green) status based on job statistics and RAG configuration.
    /// </summary>
    public interface IRagStatusEvaluator
    {
        /// <summary>
        /// Evaluates the RAG status for a set of job statistics based on the provided RAG configuration and reference date.
        /// </summary>
        /// <param name="jobStats">Collection of job statistics for a business entity.</param>
        /// <param name="ragConfig">The RAG configuration containing expressions for red, amber, and green.</param>
        /// <param name="referenceDate">The date to use for evaluating time-based rules (e.g., determining if jobs started after a certain hour).</param>
        /// <returns>An EntityStatus object containing the indicator and description of the evaluated status.</returns>
        EntityStatus Evaluate(IEnumerable<AppJobStats> jobStats, BusinessEntityRAGConfig ragConfig, DateTime referenceDate);
    }
}