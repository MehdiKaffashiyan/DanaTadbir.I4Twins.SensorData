namespace DanaTadbir.I4Twins.SensorData.Shared.Application.Mediator
{
    // TODO<Mehdi>(): Move this class to a separate Solution/Repository to enable NuGet packaging (.nupkg) for consumption across other projects.
    public interface IMediator
    {
        Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest;

        Task<TResponse> SendAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResponse>;
    }
}
