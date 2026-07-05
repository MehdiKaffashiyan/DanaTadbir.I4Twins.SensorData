using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;
using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.Constants;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Writes;

namespace DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.SensorReadings.Mappings
{
    public static class SensorReadingPointMapper
    {
        public static PointData ToPoint(this SensorReading reading)
        {
            return PointData
                .Measurement(InfluxConstants.SensorReading.Measurement)
                .Tag(InfluxConstants.SensorReading.Tags.DeviceId, reading.DeviceId)
                .Tag(InfluxConstants.SensorReading.Tags.Metric, reading.Metric.ToString().ToLowerInvariant())
                .Field(InfluxConstants.SensorReading.Fields.Value, reading.Value)
                .Field(InfluxConstants.SensorReading.Fields.Sequence, reading.Sequence)
                .Timestamp(reading.Timestamp.UtcDateTime, WritePrecision.Ms);
        }

        public static SensorReading? ToDomain(this FluxRecord record)
        {
            var deviceId = record.GetValueByKey(InfluxConstants.SensorReading.Tags.DeviceId)?.ToString();
            var metricRaw = record.GetValueByKey(InfluxConstants.SensorReading.Tags.Metric)?.ToString();
            var time = record.GetTime();

            if (string.IsNullOrWhiteSpace(deviceId) ||
                string.IsNullOrWhiteSpace(metricRaw) ||
                time is null)
            {
                return null;
            }

            if (!Enum.TryParse<MetricType>(metricRaw, ignoreCase: true, out var metric))
                return null;

            var valueObj = record.GetValueByKey(InfluxConstants.SensorReading.Fields.Value);
            if (valueObj is null)
                return null;

            var value = Convert.ToDouble(valueObj);

            var seqObj = record.GetValueByKey(InfluxConstants.SensorReading.Fields.Sequence);
            var sequence = seqObj is null ? 0 : Convert.ToInt64(seqObj);

            return SensorReading.Create(
                deviceId: deviceId,
                metric: metric,
                timestamp: time.Value.ToDateTimeOffset(),
                value: value,
                sequence: sequence
            );
        }
    }
}