using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;
using DanaTadbir.I4Twins.SensorData.Shared.Application.Mediator;

namespace DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Requests
{
    public sealed class GetAggregatedReadingsRequest : IRequest<IReadOnlyList<SensorReadingBucketDto>>
    {
        public string DeviceId { get; init; } = default!;
        public MetricType Metric { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
        public int BucketSizeSeconds { get; init; }
    }
}
