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
    public class RagStatusEvaluator_RedTests
    {
        private readonly RagStatusEvaluator _evaluator = new RagStatusEvaluator();

        // Updated RED logic: no runs, or ≥2 full failures, or one full failure + no perfect run afterwards
        private const string RedExpression =
            "!jobStats.Any() || jobStats.Count(j => j.JobStatus.ToLower() == \"fail\" && j.QualityStatus.ToLower() == \"fail\") >= 2 || (jobStats.Any(j => j.JobStatus.ToLower() == \"fail\" && j.QualityStatus.ToLower() == \"fail\") && !jobStats.Any(j => j.JobStatus.ToLower() == \"success\" && j.QualityStatus.ToLower() == \"pass\"))";

        private const string FalseExpr = "false";

        /// <summary>
        /// Scenario 0: No jobs at all → RED.
        /// </summary>
        [TestMethod]
        public void Evaluate_ReturnsRed_WhenNoJobsExist()
        {
            var config = new BusinessEntityRAGConfig
            {
                RedExpression = RedExpression,
                AmberExpression = FalseExpr,
                GreenExpression = FalseExpr
            };

            var result = _evaluator.Evaluate(
                new List<AppJobStats>(),
                config,
                DateTime.Today);

            Assert.AreEqual(RagIndicator.Red, result.Indicator);
        }

        /// <summary>
        /// Scenario 1: One full failure and no subsequent perfect run → RED.
        /// </summary>
        [TestMethod]
        public void Evaluate_ReturnsRed_WhenOneFullFailuresAndNoRecordLoaded()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats
                {
                    BusinessEntity = "BE",
                    JobStatus      = "Fail",
                    QualityStatus  = "Fail",
                    RecordLoaded   = 0,
                    RecordFailed   = 0,
                    JobStart       = day.AddHours(9),
                    JobEnd         = day.AddHours(9).AddMinutes(30),
                    RecordAsOfDate = day
                }
            };

            var config = new BusinessEntityRAGConfig
            {
                RedExpression = RedExpression,
                AmberExpression = FalseExpr,
                GreenExpression = FalseExpr
            };

            var result = _evaluator.Evaluate(jobs, config, day);

            Assert.AreEqual(RagIndicator.Red, result.Indicator);
        }

        /// <summary>
        /// Scenario 2: One full failure with failed records > 0 → RED.
        /// </summary>
        [TestMethod]
        public void Evaluate_ReturnsRed_WhenOneFullFailuresAndNoRecordLoadedWithFailedRecord()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats
                {
                    BusinessEntity = "BE",
                    JobStatus      = "Fail",
                    QualityStatus  = "Fail",
                    RecordLoaded   = 0,
                    RecordFailed   = 300,
                    JobStart       = day.AddHours(9),
                    JobEnd         = day.AddHours(9).AddMinutes(30),
                    RecordAsOfDate = day
                }
            };

            var config = new BusinessEntityRAGConfig
            {
                RedExpression = RedExpression,
                AmberExpression = FalseExpr,
                GreenExpression = FalseExpr
            };

            var result = _evaluator.Evaluate(jobs, config, day);

            Assert.AreEqual(RagIndicator.Red, result.Indicator);
        }

        /// <summary>
        /// Scenario 3: Alternating pattern of 5 runs → RED.
        /// </summary>
        [TestMethod]
        public void Evaluate_ReturnsRed_ForAlternatingFiveRunPattern()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",   QualityStatus="Fail", RecordLoaded=0,   RecordFailed=5,   JobStart=day.AddHours(8), JobEnd=day.AddHours(8).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success",QualityStatus="Pass", RecordLoaded=100, RecordFailed=0,   JobStart=day.AddHours(9), JobEnd=day.AddHours(9).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",   QualityStatus="Fail", RecordLoaded=0,   RecordFailed=10,  JobStart=day.AddHours(10),JobEnd=day.AddHours(10).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success",QualityStatus="Pass", RecordLoaded=200, RecordFailed=0,   JobStart=day.AddHours(11),JobEnd=day.AddHours(11).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success",QualityStatus="Fail", RecordLoaded=0,   RecordFailed=100, JobStart=day.AddHours(12),JobEnd=day.AddHours(12).AddMinutes(30),RecordAsOfDate=day }
            };

            var config = new BusinessEntityRAGConfig
            {
                RedExpression = RedExpression,
                AmberExpression = FalseExpr,
                GreenExpression = FalseExpr
            };
            var result = _evaluator.Evaluate(jobs, config, day);
            Assert.AreEqual(RagIndicator.Red, result.Indicator);
        }

        /// <summary>
        /// Scenario 4: Two full failures → RED.
        /// </summary>
        [TestMethod]
        public void Evaluate_ReturnsRed_WhenTwoFullFailures()
        {
            var day = new DateTime(2025, 3, 13);
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail", QualityStatus="Fail", RecordLoaded=0, RecordFailed=10, JobStart=day.AddHours(9), JobEnd=day.AddHours(9).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail", QualityStatus="Fail", RecordLoaded=0, RecordFailed=5,  JobStart=day.AddHours(10),JobEnd=day.AddHours(10).AddMinutes(30),RecordAsOfDate=day }
            };

            var config = new BusinessEntityRAGConfig
            {
                RedExpression = RedExpression,
                AmberExpression = FalseExpr,
                GreenExpression = FalseExpr
            };
            var result = _evaluator.Evaluate(jobs, config, day);
            Assert.AreEqual(RagIndicator.Red, result.Indicator);
        }

        /// <summary>
        /// Scenario 5: Full failure then quality-only failure → RED.
        /// </summary>
        [TestMethod]
        public void Evaluate_ReturnsRed_WhenFullFailureThenQualityOnlyFailure()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",   QualityStatus="Fail", RecordLoaded=0,  RecordFailed=20, JobStart=day.AddHours(8), JobEnd=day.AddHours(8).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success",QualityStatus="Fail", RecordLoaded=50, RecordFailed=5,  JobStart=day.AddHours(9), JobEnd=day.AddHours(9).AddMinutes(30),RecordAsOfDate=day }
            };

            var config = new BusinessEntityRAGConfig
            {
                RedExpression = RedExpression,
                AmberExpression = FalseExpr,
                GreenExpression = FalseExpr
            };
            var result = _evaluator.Evaluate(jobs, config, day);
            Assert.AreEqual(RagIndicator.Red, result.Indicator);
        }

        /// <summary>
        /// Scenario 6: Quality-only then full failure → RED.
        /// </summary>
        [TestMethod]
        public void Evaluate_ReturnsRed_WhenQualityOnlyThenFullFailure()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Success",QualityStatus="Fail", RecordLoaded=100,RecordFailed=1, JobStart=day.AddHours(7), JobEnd=day.AddHours(7).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",   QualityStatus="Fail", RecordLoaded=0,  RecordFailed=15,JobStart=day.AddHours(8), JobEnd=day.AddHours(8).AddMinutes(30),RecordAsOfDate=day }
            };

            var config = new BusinessEntityRAGConfig
            {
                RedExpression = RedExpression,
                AmberExpression = FalseExpr,
                GreenExpression = FalseExpr
            };
            var result = _evaluator.Evaluate(jobs, config, day);
            Assert.AreEqual(RagIndicator.Red, result.Indicator);
        }
    }
}
