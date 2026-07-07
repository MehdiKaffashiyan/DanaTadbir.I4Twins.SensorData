using DanaTadbir.I4Twins.SensorData.Api.Extensions.WebApplicationBuilders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder
    .RegisterInfrastructureServices()
    .RegisterInfluxDb()
    .RegisterRepositories()
    .RegisterApplicationHandlers()
    .RegisterHostedServices()
    .RegisterSwagger()
    .RegisterCorsPolicy();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(Cors.ApiCorsPolicy);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
