namespace DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos
{
    public sealed class SensorReadingBucketDto
    {
        public DateTimeOffset BucketStartTime { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? Average { get; set; }
        public long Count { get; set; }
    }
}
