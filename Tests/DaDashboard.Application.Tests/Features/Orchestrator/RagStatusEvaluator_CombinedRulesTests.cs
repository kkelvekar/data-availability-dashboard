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
    public class RagStatusEvaluator_CombinedRulesTests
    {
        private readonly RagStatusEvaluator _evaluator = new RagStatusEvaluator();

        // “Red” if any of:
        //   1) no runs
        //   2) ≥2 full failures (JobStatus="fail" && QualityStatus="fail")
        //   3) at least one full failure and no perfect success afterward
        private const string RedExpression =
            "!jobStats.Any() || jobStats.Count(j => j.JobStatus.ToLower() == \"fail\" && j.QualityStatus.ToLower() == \"fail\") >= 2 || (jobStats.Any(j => j.JobStatus.ToLower() == \"fail\" && j.QualityStatus.ToLower() == \"fail\") && !jobStats.Any(j => j.JobStatus.ToLower() == \"success\" && j.QualityStatus.ToLower() == \"pass\"))";

        // Amber if any data-quality failure with RecordFailed > 0
        private const string AmberExpression =
  "jobStats.Any(j => j.JobStatus.ToLower() == \"success\" && j.QualityStatus.ToLower() == \"fail\" && j.RecordFailed > 0)";


        // Green if either:
        //   1) all runs succeeded and passed quality
        //   2) first run full-fail then every subsequent run succeeded and passed quality
        private const string GreenExpression =
            "jobStats.All(j => j.JobStatus.ToLower() == \"success\" && j.QualityStatus.ToLower() == \"pass\") || (jobStats.OrderBy(j => j.JobStart).First().JobStatus.ToLower() == \"fail\" && jobStats.OrderBy(j => j.JobStart).First().QualityStatus.ToLower() == \"fail\" && jobStats.OrderBy(j => j.JobStart).Skip(1).All(j => j.JobStatus.ToLower() == \"success\" && j.QualityStatus.ToLower() == \"pass\"))";

        private BusinessEntityRAGConfig GetConfig() => new BusinessEntityRAGConfig
        {
            RedExpression = RedExpression,
            AmberExpression = AmberExpression,
            GreenExpression = GreenExpression
        };

        // --- Green scenarios ---

        [TestMethod]
        public void Evaluate_ReturnsGreen_WhenAllRunsPerfect()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=100, RecordFailed=0, JobStart=day.AddHours(8),  JobEnd=day.AddHours(8).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=150, RecordFailed=0, JobStart=day.AddHours(9),  JobEnd=day.AddHours(9).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=200, RecordFailed=0, JobStart=day.AddHours(10), JobEnd=day.AddHours(10).AddMinutes(30),RecordAsOfDate=day }
            };

            var result = _evaluator.Evaluate(jobs, GetConfig(), day);
            Assert.AreEqual(RagIndicator.Green, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Amber, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Red, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_ReturnsGreen_WhenInitialFailureThenAllPerfect()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",    QualityStatus="Fail", RecordLoaded=0,  RecordFailed=5,  JobStart=day.AddHours(7), JobEnd=day.AddHours(7).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=120,RecordFailed=0,  JobStart=day.AddHours(8), JobEnd=day.AddHours(8).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=180,RecordFailed=0,  JobStart=day.AddHours(9), JobEnd=day.AddHours(9).AddMinutes(30),RecordAsOfDate=day }
            };

            var result = _evaluator.Evaluate(jobs, GetConfig(), day);
            Assert.AreEqual(RagIndicator.Green, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Amber, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Red, result.Indicator);
        }

        // --- Amber scenarios ---

        [TestMethod]
        public void Evaluate_ReturnsAmber_WhenAllSuccessThenQualityFail()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=100, RecordFailed=0, JobStart=day.AddHours(9),  JobEnd=day.AddHours(9).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=150, RecordFailed=0, JobStart=day.AddHours(10), JobEnd=day.AddHours(10).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Fail", RecordLoaded=0,   RecordFailed=20,JobStart=day.AddHours(11), JobEnd=day.AddHours(11).AddMinutes(30),RecordAsOfDate=day }
            };

            var result = _evaluator.Evaluate(jobs, GetConfig(), day);
            Assert.AreEqual(RagIndicator.Amber, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Green, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Red, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_ReturnsAmber_ForGreenGreenAmberChain()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=120,RecordFailed=0, JobStart=day.AddHours(8),  JobEnd=day.AddHours(8).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=130,RecordFailed=0, JobStart=day.AddHours(9),  JobEnd=day.AddHours(9).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Fail", RecordLoaded=0,  RecordFailed=50,JobStart=day.AddHours(10), JobEnd=day.AddHours(10).AddMinutes(30),RecordAsOfDate=day }
            };

            var result = _evaluator.Evaluate(jobs, GetConfig(), day);
            Assert.AreEqual(RagIndicator.Amber, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Green, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Red, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_ReturnsAmber_WhenFailThenSuccessThenQualityFail()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",    QualityStatus="Fail", RecordLoaded=0, RecordFailed=10, JobStart=day.AddHours(7),  JobEnd=day.AddHours(7).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Pass", RecordLoaded=200,RecordFailed=0, JobStart=day.AddHours(8),  JobEnd=day.AddHours(8).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success", QualityStatus="Fail", RecordLoaded=0, RecordFailed=25, JobStart=day.AddHours(9),  JobEnd=day.AddHours(9).AddMinutes(30),RecordAsOfDate=day }
            };

            var result = _evaluator.Evaluate(jobs, GetConfig(), day);
            Assert.AreEqual(RagIndicator.Amber, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Green, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Red, result.Indicator);
        }

        // --- Red scenarios ---

        [TestMethod]
        public void Evaluate_ReturnsRed_WhenNoJobsExist()
        {
            var result = _evaluator.Evaluate(new List<AppJobStats>(), GetConfig(), DateTime.Today);
            Assert.AreEqual(RagIndicator.Red, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Green, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Amber, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_ReturnsRed_WhenTwoFullFailures()
        {
            var day = new DateTime(2025, 3, 13);
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail", QualityStatus="Fail", RecordLoaded=0, RecordFailed=10, JobStart=day.AddHours(9), JobEnd=day.AddHours(9).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail", QualityStatus="Fail", RecordLoaded=0, RecordFailed=5,  JobStart=day.AddHours(10),JobEnd=day.AddHours(10).AddMinutes(30),RecordAsOfDate=day }
            };

            var result = _evaluator.Evaluate(jobs, GetConfig(), day);
            Assert.AreEqual(RagIndicator.Red, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Green, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Amber, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_ReturnsRed_WhenFullFailureThenQualityOnlyFailure()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",   QualityStatus="Fail", RecordLoaded=0,  RecordFailed=20, JobStart=day.AddHours(8), JobEnd=day.AddHours(8).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success",QualityStatus="Fail", RecordLoaded=50, RecordFailed=5,  JobStart=day.AddHours(9), JobEnd=day.AddHours(9).AddMinutes(30),RecordAsOfDate=day }
            };

            var result = _evaluator.Evaluate(jobs, GetConfig(), day);
            Assert.AreEqual(RagIndicator.Red, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Green, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Amber, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_ReturnsRed_WhenQualityOnlyThenFullFailure()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Success",QualityStatus="Fail", RecordLoaded=100,RecordFailed=1, JobStart=day.AddHours(7),  JobEnd=day.AddHours(7).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",   QualityStatus="Fail", RecordLoaded=0,  RecordFailed=15,JobStart=day.AddHours(8), JobEnd=day.AddHours(8).AddMinutes(30),RecordAsOfDate=day }
            };

            var result = _evaluator.Evaluate(jobs, GetConfig(), day);
            Assert.AreEqual(RagIndicator.Red, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Green, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Amber, result.Indicator);
        }

        [TestMethod]
        public void Evaluate_ReturnsRed_ForAlternatingFiveRunPattern()
        {
            var day = DateTime.Today;
            var jobs = new List<AppJobStats>
            {
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",   QualityStatus="Fail", RecordLoaded=0,   RecordFailed=5,  JobStart=day.AddHours(8), JobEnd=day.AddHours(8).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success",QualityStatus="Pass", RecordLoaded=100, RecordFailed=0,  JobStart=day.AddHours(9), JobEnd=day.AddHours(9).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Fail",   QualityStatus="Fail", RecordLoaded=0,   RecordFailed=10, JobStart=day.AddHours(10),JobEnd=day.AddHours(10).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success",QualityStatus="Pass", RecordLoaded=200, RecordFailed=0,  JobStart=day.AddHours(11),JobEnd=day.AddHours(11).AddMinutes(30),RecordAsOfDate=day },
                new AppJobStats { BusinessEntity="BE", JobStatus="Success",QualityStatus="Fail", RecordLoaded=0,   RecordFailed=100,JobStart=day.AddHours(12),JobEnd=day.AddHours(12).AddMinutes(30),RecordAsOfDate=day }
            };

            var result = _evaluator.Evaluate(jobs, GetConfig(), day);
            Assert.AreEqual(RagIndicator.Red, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Green, result.Indicator);
            Assert.AreNotEqual(RagIndicator.Amber, result.Indicator);
        }
    }
}
