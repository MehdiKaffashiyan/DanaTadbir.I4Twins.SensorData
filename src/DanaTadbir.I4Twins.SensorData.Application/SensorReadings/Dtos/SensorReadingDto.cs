using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;

namespace DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Dtos
{
    public sealed class SensorReadingDto
    {
        public string DeviceId { get; init; } = default!;
        public MetricType Metric { get; init; }
        public double Value { get; init; }
        public long Sequence { get; init; }
        public DateTimeOffset Timestamp { get; init; }
    }
}
