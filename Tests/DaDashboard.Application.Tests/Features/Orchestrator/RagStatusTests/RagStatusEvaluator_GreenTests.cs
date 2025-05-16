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
    public class RagStatusEvaluator_GreenTests
    {
        private readonly RagStatusEvaluator _evaluator = new RagStatusEvaluator();

        /// <summary>
        /// Green if either:
        ///  1) Every run succeeded and passed quality.
        ///  2) The first run was a full failure (JobStatus == "fail" AND QualityStatus == "fail")
        ///     and all subsequent runs succeeded and passed quality.
        /// </summary>
        private const string GreenExpression =
            "jobStats.All(j => j.JobStatus.ToLower() == \"success\" && j.QualityStatus.ToLower() == \"pass\") || (jobStats.OrderBy(j => j.JobStart).First().JobStatus.ToLower() == \"fail\" && jobStats.OrderBy(j => j.JobStart).First().QualityStatus.ToLower() == \"fail\" && jobStats.OrderBy(j => j.JobStart).Skip(1).All(j => j.JobStatus.ToLower() == \"success\" && j.QualityStatus.ToLower() == \"pass\"))";

        private const string FalseExpr = "false";

        [TestMethod]
        public void Evaluate_ReturnsGreen_WhenAllRunsPerfect()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=100, RecordFailed=0,
                                  JobStart=day.AddHours(8),  JobEnd=day.AddHours(8).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=150, RecordFailed=0,
                                  JobStart=day.AddHours(9),  JobEnd=day.AddHours(9).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=200, RecordFailed=0,
                                  JobStart=day.AddHours(10), JobEnd=day.AddHours(10).AddMinutes(30),RecordAsOfDate=day }
            };
            var config = new BusinessEntityRAGConfig
            {
                RedExpression = FalseExpr,
                AmberExpression = FalseExpr,
                GreenExpression = GreenExpression
            };

            var result = _evaluator.Evaluate(jobs, config, day);
            Assert.AreEqual(RagIndicator.Green, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_ReturnsGreen_WhenInitialFailureThenAllPerfect()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",    QualityStatus="Fail", RecordLoaded=0,  RecordFailed=5,
                                  JobStart=day.AddHours(7), JobEnd=day.AddHours(7).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=120,RecordFailed=0,
                                  JobStart=day.AddHours(8), JobEnd=day.AddHours(8).AddMinutes(30), RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=180,RecordFailed=0,
                                  JobStart=day.AddHours(9), JobEnd=day.AddHours(9).AddMinutes(30), RecordAsOfDate=day }
            };
            var config = new BusinessEntityRAGConfig
            {
                RedExpression = FalseExpr,
                AmberExpression = FalseExpr,
                GreenExpression = GreenExpression
            };

            var result = _evaluator.Evaluate(jobs, config, day);
            Assert.AreEqual(RagIndicator.Green, result.Indicator);
        }
    }
}
