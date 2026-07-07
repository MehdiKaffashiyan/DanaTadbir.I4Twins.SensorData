namespace DanaTadbir.I4Twins.SensorData.Shared.Application.Mediator
{
    // TODO<Mehdi>(): Move this class to a separate Solution/Repository to enable NuGet packaging (.nupkg) for consumption across other projects.
    public interface IRequestHandler<in TRequest> where TRequest : IRequest
    {
        Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
    }

    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
