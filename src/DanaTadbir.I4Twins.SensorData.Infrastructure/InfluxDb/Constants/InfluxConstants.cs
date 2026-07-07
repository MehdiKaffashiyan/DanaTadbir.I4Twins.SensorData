namespace DanaTadbir.I4Twins.SensorData.Infrastructure.InfluxDb.Constants
{
    public static class InfluxConstants
    {
        public static class SensorReading
        {
            public const string Measurement = "sensor_reading";
            public const string Timestamp = "time";

            public static class Tags
            {
                public const string DeviceId = "device_id";
                public const string Metric = "metric";
            }

            public static class Fields
            {
                public const string Value = "value";
                public const string Sequence = "seq";
            }
        }
    }
}
