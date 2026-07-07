using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Database;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;
using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.Constants;
using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.Options;
using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.SensorReadings.Mappings;
using InfluxDB.Client;

namespace DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.SensorReadings.Repositories
{
    public sealed class InfluxSensorReadingRepository(InfluxDBClient client, InfluxDbOptions options) : ISensorReadingRepository
    {
        private readonly InfluxDBClient _client = client ?? throw new ArgumentNullException(nameof(client));
        private readonly InfluxDbOptions _options = options ?? throw new ArgumentNullException(nameof(options));

        public async Task AddAsync(SensorReading reading, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(reading);

            var point = reading.ToPoint();

            var writeApi = _client.GetWriteApiAsync();
            await writeApi.WritePointAsync(point, _options.Bucket, _options.Org, ct);
        }

        public async Task AddBatchAsync(IEnumerable<SensorReading> readings, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(readings);

            var points = readings.Select(r => r.ToPoint()).ToList();
            if (points.Count == 0)
                return;

            var writeApi = _client.GetWriteApiAsync();
            await writeApi.WritePointsAsync(points, _options.Bucket, _options.Org, ct);
        }

        public async Task<IReadOnlyList<SensorReadingBucketDto>> GetAggregatedReadingsAsync(
            string deviceId,
            MetricType metric,
            DateTimeOffset from,
            DateTimeOffset to,
            int bucketSizeSeconds,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("DeviceId is required.", nameof(deviceId));

            if (bucketSizeSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(bucketSizeSeconds), "Bucket size must be positive.");

            var metricString = metric.ToString().ToLowerInvariant();
            var flux = $@"
data = from(bucket: ""{_options.Bucket}"")
  |> range(start: {FormatFluxTime(from)}, stop: {FormatFluxTime(to)})
  |> filter(fn: (r) => r[""_measurement""] == ""{InfluxConstants.SensorReading.Measurement}"")
  |> filter(fn: (r) => r[""{InfluxConstants.SensorReading.Tags.DeviceId}""] == ""{EscapeFluxString(deviceId)}"")
  |> filter(fn: (r) => r[""{InfluxConstants.SensorReading.Tags.Metric}""] == ""{EscapeFluxString(metricString)}"")
  |> map(fn: (r) => ({{ r with _value: float(v: r._value) }}))

buckets =
  data
    |> window(every: {bucketSizeSeconds}s)
    |> reduce(
      identity: {{
        count: 0.0,
        sum: 0.0,
        min: 0.0,
        max: 0.0,
        initialized: false
      }},
      fn: (r, accumulator) => ({{
        count: accumulator.count + 1.0,
        sum: accumulator.sum + r._value,
        min: if accumulator.initialized then (if r._value < accumulator.min then r._value else accumulator.min) else r._value,
        max: if accumulator.initialized then (if r._value > accumulator.max then r._value else accumulator.max) else r._value,
        initialized: true
      }})
    )
    |> map(fn: (r) => ({{
      _time: r._stop,
      bucketStartTime: r._start,
      count: r.count,
      average: if r.count > 0.0 then r.sum / r.count else 0.0,
      minimum: if r.initialized then r.min else 0.0,
      maximum: if r.initialized then r.max else 0.0
    }}))
    |> yield(name: ""buckets"")
";

            var queryApi = _client.GetQueryApi();
            var tables = await queryApi.QueryAsync(flux, _options.Org, ct);

            var buckets = new List<SensorReadingBucketDto>();

            foreach (var record in tables.SelectMany(t => t.Records))
            {
                var bucket = record.ToBucketDto();
                if (bucket is not null)
                {
                    buckets.Add(bucket);
                }
            }

            return buckets;
        }


        #region [- Private Methods -]

        private static string FormatFluxTime(DateTimeOffset dto)
            => dto.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");

        private static string EscapeFluxString(string value)
            => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

        #endregion
    }
}