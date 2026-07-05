using Microsoft.Extensions.DependencyInjection;

namespace DanaTadbir.I4Twins.SensorData.Shared.Application.Mediator
{
    // TODO<Mehdi>(): Move this class to a separate Solution/Repository to enable NuGet packaging (.nupkg) for consumption across other projects.
    public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
    {
        public async Task SendAsync<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            ArgumentNullException.ThrowIfNull(request);

            var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
            await handler.HandleAsync(request, cancellationToken);
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResponse>
        {
            ArgumentNullException.ThrowIfNull(request);

            var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            return await handler.HandleAsync(request, cancellationToken);
        }
    }
}
