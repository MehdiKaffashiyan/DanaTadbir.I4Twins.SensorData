using AutoFixture;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;
using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.Options;
using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.SensorReadings.Repositories;
using FluentAssertions;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Moq;
using Xunit;

namespace DanaTadbir.I4Twins.SensorData.Tests.Infrastructure.InfluxDb.SensorReadings.Repositories
{
    public class InfluxSensorReadingRepositoryTests
    {
        private readonly IFixture _fixture;

        public InfluxSensorReadingRepositoryTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetAggregatedReadingsAsync_WhenDeviceIdIsNullOrWhiteSpace_ShouldThrowArgumentException()
        {
            // Arrange
            var client = new Mock<InfluxDBClient>("http://localhost", "token").Object;
            var options = _fixture.Build<InfluxDbOptions>()
                .With(x => x.Bucket, "bucket")
                .With(x => x.Org, "org")
                .Create();

            var sut = new InfluxSensorReadingRepository(client, options);

            // Act
            var act = async () => await sut.GetAggregatedReadingsAsync(
                deviceId: "   ",
                metric: MetricType.Temperature,
                from: DateTimeOffset.UtcNow.AddHours(-1),
                to: DateTimeOffset.UtcNow,
                bucketSizeSeconds: 60,
                ct: default);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .Where(ex => ex.ParamName == "deviceId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetAggregatedReadingsAsync_WhenBucketSizeSecondsIsNotPositive_ShouldThrowArgumentOutOfRangeException(int bucketSizeSeconds)
        {
            // Arrange
            var client = new Mock<InfluxDBClient>("http://localhost", "token").Object;
            var options = _fixture.Build<InfluxDbOptions>()
                .With(x => x.Bucket, "bucket")
                .With(x => x.Org, "org")
                .Create();

            var sut = new InfluxSensorReadingRepository(client, options);

            // Act
            var act = async () => await sut.GetAggregatedReadingsAsync(
                deviceId: "dev-1",
                metric: MetricType.Temperature,
                from: DateTimeOffset.UtcNow.AddHours(-1),
                to: DateTimeOffset.UtcNow,
                bucketSizeSeconds: bucketSizeSeconds,
                ct: default);

            // Assert
            await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
                .Where(ex => ex.ParamName == "bucketSizeSeconds");
        }

        // TODO<Mehdi>(GetQueryApi is non-virtual and Moq throws NotSupportedException because it cannot mock non-overridable methods without changing the production code): Refactor this to an integration test using Testcontainers or a real InfluxDB instance, as unit testing is blocked by the SDK architecture.
    }
}
