using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;

namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Database
{
    public interface ISensorReadingRepository
    {
        Task AddAsync(SensorReading reading, CancellationToken ct = default);
        Task AddBatchAsync(IEnumerable<SensorReading> readings, CancellationToken ct = default);

        Task<IReadOnlyList<SensorReadingBucketDto>> GetAggregatedReadingsAsync(
            string deviceId,
            MetricType metric,
            DateTimeOffset from,
            DateTimeOffset to,
            int bucketSizeSeconds,
            CancellationToken ct = default);
    }
}
