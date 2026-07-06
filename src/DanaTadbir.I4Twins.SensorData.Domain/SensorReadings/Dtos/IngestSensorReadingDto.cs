using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;
using System.Text.Json.Serialization;

namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos
{
    public class IngestSensorReadingDto
    {
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; init; } = default!;

        [JsonPropertyName("metric")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MetricType Metric { get; init; }

        [JsonPropertyName("ts")]
        public DateTimeOffset Timestamp { get; init; }

        [JsonPropertyName("value")]
        public double Value { get; init; }

        [JsonPropertyName("seq")]
        public long Sequence { get; init; }
    }
}
