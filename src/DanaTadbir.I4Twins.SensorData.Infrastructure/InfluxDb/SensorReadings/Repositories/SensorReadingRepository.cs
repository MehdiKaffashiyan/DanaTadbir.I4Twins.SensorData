using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Database;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;
using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.Constants;
using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.Options;
using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.SensorReadings.Mappings;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;

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

        public async Task<IEnumerable<SensorReading>> GetReadingsAsync(
            string deviceId,
            MetricType metric,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("DeviceId is required.", nameof(deviceId));

            var metricString = metric.ToString().ToLowerInvariant();

            // TODO<Mehdi>(sorting is hardcoded): get sort direction (asc/desc) from method input

            var flux = $@"
from(bucket: ""{_options.Bucket}"")
  |> range(start: {FormatFluxTime(from)}, stop: {FormatFluxTime(to)})
  |> filter(fn: (r) => r[""_measurement""] == ""{InfluxConstants.SensorReading.Measurement}"")
  |> filter(fn: (r) => r[""{InfluxConstants.SensorReading.Tags.DeviceId}""] == ""{EscapeFluxString(deviceId)}"")
  |> filter(fn: (r) => r[""{InfluxConstants.SensorReading.Tags.Metric}""] == ""{EscapeFluxString(metricString)}"")
  |> pivot(rowKey: [""_time""] columnKey: [""_field""] valueColumn: ""_value"")
  |> sort(columns: [""_time""], desc: false) 
";

            var queryApi = _client.GetQueryApi();

            var tables = await queryApi.QueryAsync(flux, _options.Org, ct);

            var readings = new List<SensorReading>();

            foreach (var record in tables.SelectMany(t => t.Records))
            {
                var reading = record.ToDomain();
                if (reading is not null)
                    readings.Add(reading);
            }

            return readings;
        }


        #region [- Private Methods -]

        private static string FormatFluxTime(DateTimeOffset dto)
            => dto.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");

        private static string EscapeFluxString(string value)
            => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

        #endregion
    }
}