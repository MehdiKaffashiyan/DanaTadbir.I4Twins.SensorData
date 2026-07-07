namespace DanaTadbir.I4Twins.SensorData.Api.Extensions.WebApplicationBuilders
{
    internal static class Swagger
    {
        internal static WebApplicationBuilder RegisterSwagger(this WebApplicationBuilder builder)
        {
            // TODO<Mehdi>(swagger is missing security setup): add JWT bearer authentication + authorization and configure Swagger security scheme/requirements

            builder.Services.AddSwaggerGen();

            return builder;
        }
    }
}
