using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DaDashboard.Application.Features.Orchestrator;
using DaDashboard.Application.Contracts.Application.Orchestrator;

namespace DaDashboard.Application.Tests.Features.Orchestrator
{
    [TestClass]
    public class JobStatsStrategyFactoryTests
    {
        [TestMethod]
        public void GetStrategy_ReturnsExpectedStrategy_WhenNameMatchesIgnoringCase()
        {
            // Arrange
            var strategyMock = new Mock<IJobStatsStrategy>();
            strategyMock.Setup(s => s.StrategyName).Returns("MyStrategy");
            var strategies = new List<IJobStatsStrategy> { strategyMock.Object };
            var factory = new JobStatsStrategyFactory(strategies);

            // Act
            var result = factory.GetStrategy("mystrategy");

            // Assert
            Assert.AreSame(strategyMock.Object, result, "Factory should return the matching strategy regardless of case.");
        }

        [TestMethod]
        public void GetStrategy_ThrowsInvalidOperationException_WhenStrategyNotFound()
        {
            // Arrange
            var factory = new JobStatsStrategyFactory(new List<IJobStatsStrategy>());

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(
                () => factory.GetStrategy("UnknownStrategy"),
                "Factory should throw when no matching strategy is registered.");
        }
    }
}