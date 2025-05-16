using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DaDashboard.Application.Features.Orchestrator;
using DaDashboard.Domain.Entities;
using DaDashboard.Domain;
using AppJobStats = DaDashboard.Application.Models.Infrastructure.DataLoadStatistics.JobStats;

namespace DaDashboard.Application.Tests.Features.Orchestrator
{
    [TestClass]
    public class RagStatusEvaluator_AmberTests
    {
        private readonly RagStatusEvaluator _evaluator = new RagStatusEvaluator();

        // Amber if any data-quality failure (failed rows > 0)
        private const string AmberExpression =
            "jobStats.Any(j => j.QualityStatus.ToLower() == \"fail\" && j.RecordFailed > 0)";

        private const string FalseExpr = "false";

        /// <summary>
        /// Scenario 1: All-success then one quality-failure → AMBER.
        /// Runs: Success/Pass, Success/Pass, Success/Fail (>0).
        /// </summary>
        [TestMethod]
        public void Evaluate_ReturnsAmber_WhenAllSuccessThenQualityFail()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=100, RecordFailed=0,
                                  JobStart=day.AddHours(9),  JobEnd=day.AddHours(9).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=150, RecordFailed=0,
                                  JobStart=day.AddHours(10), JobEnd=day.AddHours(10).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Fail", RecordLoaded=0,   RecordFailed=20,
                                  JobStart=day.AddHours(11), JobEnd=day.AddHours(11).AddMinutes(30), RecordAsOfDate=day }
            };

            var config = new BusinessEntityRAGConfig
            {
                RedExpression = FalseExpr,
                AmberExpression = AmberExpression,
                GreenExpression = FalseExpr
            };

            var result = _evaluator.Evaluate(jobs, config, day);
            Assert.AreEqual(RagIndicator.Amber, result.Indicator);
        }

        /// <summary>
        /// Scenario 2: Green → Green → Amber chain → AMBER.
        /// Runs: Success/Pass, Success/Pass, Success/Fail (>0).
        /// </summary>
        [TestMethod]
        public void Evaluate_ReturnsAmber_ForGreenGreenAmberChain()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=120, RecordFailed=0,
                                  JobStart=day.AddHours(8),  JobEnd=day.AddHours(8).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=130, RecordFailed=0,
                                  JobStart=day.AddHours(9),  JobEnd=day.AddHours(9).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Fail", RecordLoaded=0,   RecordFailed=50,
                                  JobStart=day.AddHours(10), JobEnd=day.AddHours(10).AddMinutes(30),RecordAsOfDate=day }
            };

            var config = new BusinessEntityRAGConfig
            {
                RedExpression = FalseExpr,
                AmberExpression = AmberExpression,
                GreenExpression = FalseExpr
            };

            var result = _evaluator.Evaluate(jobs, config, day);
            Assert.AreEqual(RagIndicator.Amber, result.Indicator);
        }

        /// <summary>
        /// Scenario 3: Red → Green → Amber chain → AMBER.
        /// Runs: Fail/Fail, Success/Pass, Success/Fail (>0).
        /// </summary>
        [TestMethod]
        public void Evaluate_ReturnsAmber_WhenFailThenSuccessThenQualityFail()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",    QualityStatus="Fail", RecordLoaded=0, RecordFailed=10,
                                  JobStart=day.AddHours(7),  JobEnd=day.AddHours(7).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=200,RecordFailed=0,
                                  JobStart=day.AddHours(8),  JobEnd=day.AddHours(8).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Fail", RecordLoaded=0, RecordFailed=25,
                                  JobStart=day.AddHours(9),  JobEnd=day.AddHours(9).AddMinutes(30), RecordAsOfDate=day }
            };

            var config = new BusinessEntityRAGConfig
            {
                RedExpression = FalseExpr,
                AmberExpression = AmberExpression,
                GreenExpression = FalseExpr
            };

            var result = _evaluator.Evaluate(jobs, config, day);
            Assert.AreEqual(RagIndicator.Amber, result.Indicator);
        }
    }
}
