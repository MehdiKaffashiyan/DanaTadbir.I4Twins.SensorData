using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Database;
using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.SensorReadings.Repositories;

namespace DanaTadbir.I4Twins.SensorData.Api.Extensions.WebApplicationBuilders
{
    internal static class Repositories
    {
        internal static WebApplicationBuilder RegisterRepositories(this WebApplicationBuilder builder)
        {
            #region [- SensorReadings -]
            builder.Services.AddScoped<ISensorReadingRepository, InfluxSensorReadingRepository>();
            #endregion

            return builder;
        }
    }
}
