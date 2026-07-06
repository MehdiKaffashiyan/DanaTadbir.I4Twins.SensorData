using DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Requests;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Database;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Shared.Application.Mediator;

namespace DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Handlers
{
    public sealed class GetAggregatedReadingsHandler(ISensorReadingRepository sensorReadingRepository)
    : IRequestHandler<GetAggregatedReadingsRequest, IReadOnlyList<SensorReadingBucketDto>>
    {
        public async Task<IReadOnlyList<SensorReadingBucketDto>> HandleAsync(
            GetAggregatedReadingsRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await sensorReadingRepository.GetAggregatedReadingsAsync(
                request.DeviceId,
                request.Metric,
                request.From,
                request.To,
                request.BucketSizeSeconds,
                cancellationToken);

            return result;
        }
    }
}
