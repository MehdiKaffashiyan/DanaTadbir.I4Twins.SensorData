using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;

namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities
{
    public sealed record SensorReadingKey(
        string DeviceId,
        MetricType Metric,
        DateTimeOffset Timestamp,
        long Sequence);
}
