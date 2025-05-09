using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DaDashboard.Application.Features.Orchestrator;
using DaDashboard.Domain.Entities;
using DaDashboard.Domain;
using AppJobStats = DaDashboard.Application.Models.Infrastructure.DataLoadStatistics.JobStats;

namespace DaDashboard.Application.Tests.Features.Orchestrator
{
    [TestClass]
    public class RagStatusEvaluatorTests
    {
        private readonly RagStatusEvaluator _evaluator = new RagStatusEvaluator();

        [TestMethod]
        public void Evaluate_ReturnsRed_WhenRedConditionMet()
        {
            // Arrange
            var config = new BusinessEntityRAGConfig
            {
                RedExpression = "jobStats.Any(j => j.JobStatus.ToLower() == \"failure\")",
                AmberExpression = "false",
                GreenExpression = "false"
            };
            var jobStats = new List<AppJobStats>
            {
                new AppJobStats { JobStatus = "Failure", QualityStatus = "Pass", JobStart = DateTime.Now.AddHours(-1) }
            };
            var referenceDate = DateTime.Now;

            // Act
            var result = _evaluator.Evaluate(jobStats, config, referenceDate);

            // Assert
            Assert.AreEqual(RagIndicator.Red, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_ReturnsAmber_WhenAmberConditionMet()
        {
            // Arrange
            var config = new BusinessEntityRAGConfig
            {
                RedExpression = "false",
                AmberExpression = "jobStats.All(j => j.JobStatus.ToLower() == \"success\") && jobStats.Any(j => j.QualityStatus.ToLower() == \"fail\")",
                GreenExpression = "false"
            };
            var jobStats = new List<AppJobStats>
            {
                new AppJobStats { JobStatus = "Success", QualityStatus = "Fail", JobStart = DateTime.Now.AddHours(-2) },
                new AppJobStats { JobStatus = "Success", QualityStatus = "Fail", JobStart = DateTime.Now.AddHours(-1) }
            };
            var referenceDate = DateTime.Today;

            // Act
            var result = _evaluator.Evaluate(jobStats, config, referenceDate);

            // Assert
            Assert.AreEqual(RagIndicator.Amber, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_ReturnsGreen_WhenGreenConditionMet()
        {
            // Arrange
            var config = new BusinessEntityRAGConfig
            {
                RedExpression = "false",
                AmberExpression = "false",
                GreenExpression = "jobStats.All(j => j.JobStatus.ToLower() == \"success\") && jobStats.All(j => j.QualityStatus.ToLower() == \"pass\")"
            };
            var jobStats = new List<AppJobStats>
            {
                new AppJobStats { JobStatus = "Success", QualityStatus = "Pass", JobStart = DateTime.Now }
            };
            var referenceDate = DateTime.UtcNow;

            // Act
            var result = _evaluator.Evaluate(jobStats, config, referenceDate);

            // Assert
            Assert.AreEqual(RagIndicator.Green, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_FallbacksToGreen_WhenNoConditionMatched()
        {
            // Arrange
            var config = new BusinessEntityRAGConfig
            {
                RedExpression = "false",
                AmberExpression = "false",
                GreenExpression = "false"
            };
            var jobStats = new List<AppJobStats>
            {
                new AppJobStats { JobStatus = "Unknown", QualityStatus = "Unknown", JobStart = DateTime.Now }
            };
            var referenceDate = DateTime.Now;

            // Act
            var result = _evaluator.Evaluate(jobStats, config, referenceDate);

            // Assert
            Assert.AreEqual(RagIndicator.Green, result.Indicator);
        }
        
        [TestMethod]
        public void Evaluate_UsesCurrentDateForStartTimeRule()
        {
            // Arrange: Red if no job starts after 9 AM on the reference date
            var config = new BusinessEntityRAGConfig
            {
                RedExpression = "jobStats.Any(j => j.JobStatus.ToLower() == \"failure\") || !jobStats.Any(j => j.JobStart >= currentDate.AddHours(9))",
                AmberExpression = "false",
                GreenExpression = "false"
            };
            var referenceDate = new DateTime(2025, 4, 1);

            // Act/Assert: job before 9 AM should be Red
            var statsBefore9 = new List<AppJobStats>
            {
                new AppJobStats { JobStatus = "Success", QualityStatus = "Pass", JobStart = referenceDate.AddHours(8) }
            };
            var resultBefore9 = _evaluator.Evaluate(statsBefore9, config, referenceDate);
            Assert.AreEqual(RagIndicator.Red, resultBefore9.Indicator);

            // Act/Assert: job at or after 9 AM should not be Red (fallback to Green)
            var statsAfter9 = new List<AppJobStats>
            {
                new AppJobStats { JobStatus = "Success", QualityStatus = "Pass", JobStart = referenceDate.AddHours(9) }
            };
            var resultAfter9 = _evaluator.Evaluate(statsAfter9, config, referenceDate);
            Assert.AreEqual(RagIndicator.Green, resultAfter9.Indicator);
        }
    }
}