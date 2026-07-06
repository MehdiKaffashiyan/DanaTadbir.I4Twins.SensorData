namespace DanaTadbir.I4Twins.SensorData.Api.Extensions.WebApplicationBuilders
{
    internal static class Cors
    {
        private static List<string> AllowedOriginHost = new List<string>() { "localhost" };
        public const string ApiCorsPolicy = "ApiCorsPolicy";

        internal static WebApplicationBuilder RegisterCorsPolicy(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(ApiCorsPolicy,
                    builder => builder
                        .SetIsOriginAllowed(origin => AllowedOriginHost.Contains(new Uri(origin).Host))
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("X-Pagination")
                        .AllowCredentials()
                );
            });

            return builder;
        }
    }
}
