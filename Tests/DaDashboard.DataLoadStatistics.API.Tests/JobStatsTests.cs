using System;
using System.Linq;
using DataLoadStatistics.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DaDashboard.DataLoadStatistics.API.Tests
{
    [TestClass]
    public class JobStatsTests
    {
        [TestMethod]
        public void GenerateRandomJobStats_StaticData_HasFiveRecordsPerEntityAndCorrectDate()
        {
            // Arrange
            var date = new DateTime(2025, 1, 1);

            // Act
            var stats = JobStats.GenerateRandomJobStats(date);

            // Assert
            const int expectedEntities = 14;
            const int recordsPerEntity = 5;
            int expectedCount = expectedEntities * recordsPerEntity;
            Assert.AreEqual(expectedCount, stats.Count, $"Expected {expectedCount} records but found {stats.Count}");
            Assert.IsTrue(stats.All(s => s.RecordAsOfDate == date), "All records should have the provided RecordAsOfDate");
        }
    }
}