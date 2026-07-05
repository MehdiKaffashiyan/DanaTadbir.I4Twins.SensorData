using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Exceptions;
using DanaTadbir.I4Twins.SensorData.Shared.Domain;

namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;

public sealed class SensorReading : AggregateRootBase<Guid>
{
    public string DeviceId { get; private set; }
    public string Metric { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public double Value { get; private set; }
    public long Sequence { get; private set; }

    private SensorReading(
        string deviceId,
        string metric,
        DateTimeOffset timestamp,
        double value,
        long sequence)
    {
        DeviceId = deviceId;
        Metric = metric;
        Timestamp = timestamp;
        Value = value;
        Sequence = sequence;

        Validate();
    }

    public static SensorReading Create(
        string deviceId,
        string metric,
        DateTimeOffset timestamp,
        double value,
        long sequence)
        => new(deviceId, metric, timestamp, value, sequence);

    public SensorReadingKey GetKey() => new(DeviceId, Metric, Timestamp, Sequence);

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(DeviceId))
            throw new InvalidReadingException("DeviceId is required.");


        // TODO<Mehdi>(): add other validations
    }
}
