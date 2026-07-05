using DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Enums;
using DanaTadbir.I4Twins.SensorData.Shared.Application.Common;
using DanaTadbir.I4Twins.SensorData.Shared.Application.Mediator;

namespace DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Requests
{
    public sealed class GetSensorReadingsRequest : IRequest<IReadOnlyList<SensorReadingDto>>
    {
        public string DeviceId { get; init; } = default!;
        public MetricType Metric { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
        public SortOrder SortOrder { get; init; } = SortOrder.Asc;
    }
}
