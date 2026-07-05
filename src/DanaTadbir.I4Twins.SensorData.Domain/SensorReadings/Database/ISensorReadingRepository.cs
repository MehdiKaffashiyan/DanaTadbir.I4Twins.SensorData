using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;

namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Database
{
    public interface ISensorReadingRepository
    {
        Task AddAsync(SensorReading reading, CancellationToken ct = default);
        Task AddBatchAsync(IEnumerable<SensorReading> readings, CancellationToken ct = default);

        Task<IEnumerable<SensorReading>> GetReadingsAsync(
            string deviceId,
            string metric,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken ct = default);
    }
}
