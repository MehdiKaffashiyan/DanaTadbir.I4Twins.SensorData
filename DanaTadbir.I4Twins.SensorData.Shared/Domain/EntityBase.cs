namespace DanaTadbir.I4Twins.SensorData.Shared.Domain
{
    // TODO<Mehdi>(): Move this class to a separate Solution/Repository to enable NuGet packaging (.nupkg) for consumption across other projects.
    public abstract class EntityBase<TId>
    {
        public TId Id { get; protected set; }
    }
}
