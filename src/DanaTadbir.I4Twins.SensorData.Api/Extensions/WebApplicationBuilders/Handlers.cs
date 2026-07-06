using DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Handlers;
using DanaTadbir.I4Twins.SensorData.Application.SensorReadings.Requests;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Shared.Application.Mediator;

namespace DanaTadbir.I4Twins.SensorData.Api.Extensions.WebApplicationBuilders
{
    internal static class Handlers
    {
        internal static WebApplicationBuilder RegisterApplicationHandlers(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IMediator, Mediator>();

            #region [- SensorReadings -]
            builder.Services.AddTransient<IRequestHandler<GetAggregatedReadingsRequest, IReadOnlyList<SensorReadingBucketDto>>, GetAggregatedReadingsHandler>();
            #endregion

            return builder;
        }
    }
}
