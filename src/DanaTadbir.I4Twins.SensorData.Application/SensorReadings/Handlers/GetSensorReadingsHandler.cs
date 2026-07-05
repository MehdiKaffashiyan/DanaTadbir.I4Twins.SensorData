using DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Requests;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Database;
using DanaTadbir.I4Twins.SensorData.Shared.Application.Mediator;

namespace DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Handlers
{
    public sealed class GetSensorReadingsHandler(ISensorReadingRepository sensorReadingRepository)
                : IRequestHandler<GetSensorReadingsRequest, IReadOnlyList<SensorReadingDto>>
    {
        public async Task<IReadOnlyList<SensorReadingDto>> HandleAsync(
            GetSensorReadingsRequest request,
            CancellationToken cancellationToken = default)
        {
            // TODO<Mehdi>(sorting is hardcoded): pass request.SortOrder down to repository/query layer

            var readings = await sensorReadingRepository.GetReadingsAsync(
                request.DeviceId,
                request.Metric,
                request.From,
                request.To,
                cancellationToken);

            return [.. readings
                .Select(reading => new SensorReadingDto
                {
                    DeviceId = reading.DeviceId,
                    Metric = reading.Metric,
                    Value = reading.Value,
                    Sequence = reading.Sequence,
                    Timestamp = reading.Timestamp
                })];
        }
    }
}
