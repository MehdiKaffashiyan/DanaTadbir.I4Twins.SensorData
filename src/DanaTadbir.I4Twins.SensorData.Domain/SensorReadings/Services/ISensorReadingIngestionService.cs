using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos;

namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Services
{
    public interface ISensorReadingIngestionService
    {
        Task IngestAsync(
            IngestSensorReadingDto dto,
            CancellationToken cancellationToken = default);

        Task IngestBatchAsync(
            IReadOnlyCollection<IngestSensorReadingDto> dtos,
            CancellationToken cancellationToken = default);
    }
}
