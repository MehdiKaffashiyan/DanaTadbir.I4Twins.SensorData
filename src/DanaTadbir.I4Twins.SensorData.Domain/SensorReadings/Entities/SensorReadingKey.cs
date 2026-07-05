namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities
{
    public sealed record SensorReadingKey(
        string DeviceId,
        string Metric,
        DateTimeOffset Timestamp,
        long Sequence);
}
