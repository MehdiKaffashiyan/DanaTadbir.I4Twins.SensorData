using DanaTadbir.I4Twins.SensorData.Shared.Domain;

namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Exceptions
{
    public sealed class InvalidReadingException(string message) : DomainException(message)
    {
    }
}
