using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Database;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Services;

namespace DanaTadbir.I4Twins.SensorData.Infrastructure.Services
{
    public sealed class SensorReadingIngestionService : ISensorReadingIngestionService
    {
        private readonly ISensorReadingRepository _sensorReadingRepository;

        public SensorReadingIngestionService(ISensorReadingRepository sensorReadingRepository)
        {
            _sensorReadingRepository = sensorReadingRepository;
        }

        public async Task IngestAsync(
            IngestSensorReadingDto dto,
            CancellationToken cancellationToken = default)
        {
            var reading = SensorReading.Create(
                dto.DeviceId,
                dto.Metric,
                dto.Timestamp,
                dto.Value,
                dto.Sequence);

            await _sensorReadingRepository.AddAsync(reading, cancellationToken);
        }

        public async Task IngestBatchAsync(
            IReadOnlyCollection<IngestSensorReadingDto> dtos,
            CancellationToken cancellationToken = default)
        {
            var readings = dtos
                .Select(dto => SensorReading.Create(
                    dto.DeviceId,
                    dto.Metric,
                    dto.Timestamp,
                    dto.Value,
                    dto.Sequence))
                .ToList();

            await _sensorReadingRepository.AddBatchAsync(readings, cancellationToken);
        }
    }
}
