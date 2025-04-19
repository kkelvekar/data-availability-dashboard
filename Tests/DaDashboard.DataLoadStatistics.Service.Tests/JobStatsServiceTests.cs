using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DaDashboard.Application.Contracts.Infrastructure.DataLoadStatistics;
using DaDashboard.Application.Models.Infrastructure.DataLoadStatistics;
using DaDashboard.DataLoadStatistics.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace DaDashboard.DataLoadStatistics.Service.Tests
{
    [TestClass]
    public class JobStatsServiceTests
    {
        [TestMethod]
        public void Constructor_NullFactory_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new JobStatsService(null!));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_NullFilter_ThrowsArgumentNullException()
        {
            var factoryMock = new Mock<IHttpClientFactory>();
            var service = new JobStatsService(factoryMock.Object);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                () => service.GetJobStatsAsync(null!, "http://test"));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task GetJobStatsAsync_InvalidBaseUrl_ThrowsArgumentNullException(string baseUrl)
        {
            var factoryMock = new Mock<IHttpClientFactory>();
            var service = new JobStatsService(factoryMock.Object);
            var filter = new JobStatsRequest { BusinessEntities = new List<string> { "BE1" } };
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                () => service.GetJobStatsAsync(filter, baseUrl!));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_EmptyBusinessEntities_ThrowsArgumentException()
        {
            var factoryMock = new Mock<IHttpClientFactory>();
            var service = new JobStatsService(factoryMock.Object);
            var filter = new JobStatsRequest { BusinessEntities = new List<string>(), RecordAsOfDate = null };
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => service.GetJobStatsAsync(filter, "http://test"));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_HttpRequestException_IsWrapped()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(   
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var client = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("JobStatsClient")).Returns(client);

            var service = new JobStatsService(factoryMock.Object);
            var filter = new JobStatsRequest { BusinessEntities = new List<string> { "BE1" } };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => service.GetJobStatsAsync(filter, "http://localhost"));
        }

        [TestMethod]
        public async Task GetJobStatsAsync_ValidResponse_ReturnsList()
        {
            // Arrange
            var expected = new List<JobStats>
            {
                new JobStats { BusinessEntity = "Entity1", RecordLoaded = 5, RecordAsOfDate = DateTime.Parse("2025-01-01T00:00:00Z") }
            };
            var json = JsonSerializer.Serialize(expected, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(  
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("JobStatsClient")).Returns(client);

            var service = new JobStatsService(factoryMock.Object);
            var filter = new JobStatsRequest { BusinessEntities = new List<string> { "Entity1" } };

            // Act
            var result = await service.GetJobStatsAsync(filter, "http://localhost");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Entity1", result[0].BusinessEntity);
            Assert.AreEqual(5, result[0].RecordLoaded);
            Assert.AreEqual(DateTime.Parse("2025-01-01T00:00:00Z"), result[0].RecordAsOfDate);
        }
    }
}