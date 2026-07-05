using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;

namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Services
{
    public interface IDeduplicationService
    {
        Task<bool> IsDuplicateAsync(SensorReadingKey key, CancellationToken cancellationToken = default);
        Task MarkAsProcessedAsync(SensorReadingKey key, CancellationToken cancellationToken = default);
    }
}
