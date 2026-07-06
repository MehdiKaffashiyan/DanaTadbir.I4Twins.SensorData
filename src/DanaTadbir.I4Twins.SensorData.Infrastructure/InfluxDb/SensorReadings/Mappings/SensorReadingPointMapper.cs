using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos;
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

        public static SensorReadingBucketDto? ToBucketDto(this FluxRecord record)
        {
            if (record is null) return null;

            var time = record.GetTime();
            if (time is null)
                return null;

            var bucketStart = time.Value.ToDateTimeOffset();

            var min = TryGetDouble(record, "min") ?? TryGetDouble(record, "minimum");
            var max = TryGetDouble(record, "max") ?? TryGetDouble(record, "maximum");
            var avg = TryGetDouble(record, "avg")
                      ?? TryGetDouble(record, "average")
                      ?? TryGetDouble(record, "mean");
            var count = TryGetLong(record, "count");

            if (min is null && max is null && avg is null && count is null)
            {
                var field = record.GetValueByKey("_field")?.ToString();
                var valueObj = record.GetValue();

                if (string.IsNullOrWhiteSpace(field))
                    return null;

                var dto = new SensorReadingBucketDto
                {
                    BucketStartTime = bucketStart,
                    Count = 0
                };

                switch (field.Trim().ToLowerInvariant())
                {
                    case "min":
                    case "minimum":
                        dto.Min = TryConvertToDouble(valueObj);
                        break;

                    case "max":
                    case "maximum":
                        dto.Max = TryConvertToDouble(valueObj);
                        break;

                    case "mean":
                    case "avg":
                    case "average":
                        dto.Average = TryConvertToDouble(valueObj);
                        break;

                    case "count":
                        dto.Count = TryConvertToLong(valueObj) ?? 0;
                        break;

                    default:
                        return null;
                }

                return dto;
            }

            return new SensorReadingBucketDto
            {
                BucketStartTime = bucketStart,
                Count = count ?? 0,
                Average = avg,
                Min = min,
                Max = max
            };
        }

        private static double? TryGetDouble(FluxRecord record, string key)
            => TryConvertToDouble(record.GetValueByKey(key));

        private static long? TryGetLong(FluxRecord record, string key)
            => TryConvertToLong(record.GetValueByKey(key));

        private static double? TryConvertToDouble(object? value)
        {
            if (value is null) return null;
            try { return Convert.ToDouble(value); }
            catch { return null; }
        }

        private static long? TryConvertToLong(object? value)
        {
            if (value is null) return null;
            try { return Convert.ToInt64(value); }
            catch { return null; }
        }
    }
}   