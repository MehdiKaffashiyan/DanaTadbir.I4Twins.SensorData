using DanaTadbir.I4Twins.SensorData.Api.BackgroundJobs;

namespace DanaTadbir.I4Twins.SensorData.Api.Extensions.WebApplicationBuilders
{
    internal static class HostedServices
    {
        internal static WebApplicationBuilder RegisterHostedServices(this WebApplicationBuilder builder)
        {
            var worker = builder.Configuration.GetSection("Worker");

            builder.Services.Configure<SensorReadingIngestionWorker.WorkerOptions>(worker);

            builder.Services.AddHostedService<SensorReadingIngestionWorker>();

            return builder;
        }
    }
}
