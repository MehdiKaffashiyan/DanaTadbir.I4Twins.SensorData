using AutoFixture;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;
using DanaTadbir.I4Twins.SensorData.Infrastructure.Deduplication.SensorReadings;
using FluentAssertions;
using Xunit;

namespace DanaTadbir.I4Twins.SensorData.Tests.Infrastructure.Deduplication.SensorReadings
{
    public class InMemoryDeduplicationServiceTests
    {
        private readonly IFixture _fixture;
        private readonly InMemoryDeduplicationService _sut;

        public InMemoryDeduplicationServiceTests()
        {
            _fixture = new Fixture();
            _sut = new InMemoryDeduplicationService();
        }

        [Fact]
        public async Task IsDuplicateAsync_WhenKeyDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var key = _fixture.Create<SensorReadingKey>();

            // Act
            var result = await _sut.IsDuplicateAsync(key, TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsDuplicateAsync_WhenKeyExists_ShouldReturnTrue()
        {
            // Arrange
            var key = _fixture.Create<SensorReadingKey>();
            await _sut.MarkAsProcessedAsync(key, TestContext.Current.CancellationToken);

            // Act
            var result = await _sut.IsDuplicateAsync(key, TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task MarkAsProcessedAsync_ShouldAddKeyToStore()
        {
            // Arrange
            var key = _fixture.Create<SensorReadingKey>();

            // Act
            await _sut.MarkAsProcessedAsync(key, TestContext.Current.CancellationToken);

            // Assert
            var isDuplicate = await _sut.IsDuplicateAsync(key, TestContext.Current.CancellationToken);
            isDuplicate.Should().BeTrue();
        }

        [Fact]
        public async Task MarkAsProcessedAsync_WhenCalledMultipleTimesWithSameKey_ShouldNotThrow()
        {
            // Arrange
            var key = _fixture.Create<SensorReadingKey>();

            await _sut.MarkAsProcessedAsync(key, TestContext.Current.CancellationToken);

            // Act
            var act = async () => await _sut.MarkAsProcessedAsync(key, TestContext.Current.CancellationToken);

            // Assert
            await act.Should().NotThrowAsync("some error message.");
        }

        [Fact]
        public async Task MarkAsProcessedAsync_ShouldBeThreadSafe()
        {
            // Arrange
            var key = _fixture.Create<SensorReadingKey>();
            int concurrentTasks = 50;

            // Act
            var tasks = Enumerable.Range(0, concurrentTasks)
                .Select(_ => _sut.MarkAsProcessedAsync(key, TestContext.Current.CancellationToken));

            var act = async () => await Task.WhenAll(tasks);

            // Assert
            await act.Should().NotThrowAsync();

            var isDuplicate = await _sut.IsDuplicateAsync(key, TestContext.Current.CancellationToken);
            isDuplicate.Should().BeTrue();
        }
    }
}
