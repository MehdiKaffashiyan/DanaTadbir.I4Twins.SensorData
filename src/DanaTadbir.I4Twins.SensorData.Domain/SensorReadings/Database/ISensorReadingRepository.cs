using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;

namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Database
{
    public interface ISensorReadingRepository
    {
        Task AddAsync(SensorReading reading, CancellationToken ct = default);
        Task AddBatchAsync(IEnumerable<SensorReading> readings, CancellationToken ct = default);

        Task<IEnumerable<SensorReading>> GetReadingsAsync(
            string deviceId,
            MetricType metric,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken ct = default);
    }
}
