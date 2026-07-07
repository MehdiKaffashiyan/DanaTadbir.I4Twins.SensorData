using DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.Options;
using InfluxDB.Client;

namespace DanaTadbir.I4Twins.SensorData.Api.Extensions.WebApplicationBuilders
{
    internal static class InfluxDb
    {
        internal static WebApplicationBuilder RegisterInfluxDb(this WebApplicationBuilder builder)
        {
            var options = builder.Configuration.GetSection("InfluxDb").Get<InfluxDbOptions>()
                ?? throw new InvalidOperationException("InfluxDb configuration is missing.");

            builder.Services.AddSingleton(options);

            builder.Services.AddSingleton(sp =>
            {
                return new InfluxDBClient(options.Url, options.Token);
            });

            return builder;
        }
    }
}
