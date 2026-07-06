using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Services;
using System.Collections.Concurrent;

namespace DanaTadbir.I4Twins.SensorData.Infrastructure.Deduplication.SensorReadings
{
    // TODO<Mehdi>(replace in-memory deduplication): use Redis for distributed deduplication
    public sealed class InMemoryDeduplicationService : IDeduplicationService
    {
        private readonly ConcurrentDictionary<SensorReadingKey, byte> _processedKeys = new();

        public Task<bool> IsDuplicateAsync(SensorReadingKey key, CancellationToken cancellationToken = default)
        {
            var exists = _processedKeys.ContainsKey(key);
            return Task.FromResult(exists);
        }

        public Task MarkAsProcessedAsync(SensorReadingKey key, CancellationToken cancellationToken = default)
        {
            _processedKeys.TryAdd(key, default);
            return Task.CompletedTask;
        }
    }
}
