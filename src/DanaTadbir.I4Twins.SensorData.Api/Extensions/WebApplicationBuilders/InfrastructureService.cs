using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Services;
using DanaTadbir.I4Twins.SensorData.Infrastructure.Deduplication.SensorReadings;
using DanaTadbir.I4Twins.SensorData.Infrastructure.Services;

namespace DanaTadbir.I4Twins.SensorData.Api.Extensions.WebApplicationBuilders
{
    internal static class InfrastructureService
    {
        internal static WebApplicationBuilder RegisterInfrastructureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IDeduplicationService, InMemoryDeduplicationService>();
            builder.Services.AddScoped<ISensorReadingIngestionService, SensorReadingIngestionService>();

            return builder;
        }
    }
}
