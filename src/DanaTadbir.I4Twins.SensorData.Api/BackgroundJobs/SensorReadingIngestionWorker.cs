using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Dtos;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Entities;
using DanaTadbir.I4Twins.SensorData.Domain.SensorReadings.Services;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanaTadbir.I4Twins.SensorData.Api.BackgroundJobs
{
    public sealed class SensorReadingIngestionWorker : BackgroundService
    {
        private readonly ILogger<SensorReadingIngestionWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly WorkerOptions _options;
        private readonly JsonSerializerOptions _serializerOptions;

        public SensorReadingIngestionWorker(
            ILogger<SensorReadingIngestionWorker> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<WorkerOptions> options)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _options = options.Value;

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await ProcessFileAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SensorReadingIngestionWorker was canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in SensorReadingIngestionWorker.");
            }
        }

        private async Task ProcessFileAsync(CancellationToken stoppingToken)
        {
            var inputFilePath = ResolveFilePath(_options.ReadingsFilePath);

            if (!File.Exists(inputFilePath))
            {
                _logger.LogWarning("Input file not found: {FilePath}", inputFilePath);
                return;
            }

            var outputDirectory = ResolveOutputDirectory(_options.OutputDirectory);
            Directory.CreateDirectory(outputDirectory);

            var summaryFilePath = Path.Combine(outputDirectory, _options.SummaryFileName);

            using var scope = _scopeFactory.CreateScope();
            var ingestionService = scope.ServiceProvider.GetRequiredService<ISensorReadingIngestionService>();
            var dedupService = scope.ServiceProvider.GetRequiredService<IDeduplicationService>();

            var stats = new IngestionSummary
            {
                InputFilePath = inputFilePath,
                OutputDirectory = outputDirectory
            };

            var buffer = new List<BufferedReading>(_options.WindowSize);
            var seenInRun = new HashSet<SensorReadingKey>();

            await using var stream = new FileStream(
                inputFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            using var reader = new StreamReader(stream);

            string? line;
            long lineNumber = 0;

            while ((line = await reader.ReadLineAsync(stoppingToken)) is not null &&
                   !stoppingToken.IsCancellationRequested)
            {
                lineNumber++;
                stats.TotalLinesRead++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    stats.EmptyLineCount++;
                    stats.InvalidCount++;
                    continue;
                }

                if (!TryDeserialize(line, out var dto))
                {
                    stats.InvalidCount++;
                    continue;
                }

                if (!TryValidate(dto!, out _))
                {
                    stats.InvalidCount++;
                    continue;
                }

                var key = new SensorReadingKey(dto!.DeviceId, dto.Metric, dto.Timestamp, dto.Sequence);

                if (!seenInRun.Add(key))
                {
                    stats.DuplicateCount++;
                    continue;
                }

                if (await dedupService.IsDuplicateAsync(key, stoppingToken))
                {
                    stats.DuplicateCount++;
                    continue;
                }

                buffer.Add(new BufferedReading(dto, key));

                if (buffer.Count >= _options.WindowSize)
                {
                    stats.IngestedCount += await FlushBufferedWindowAsync(
                        buffer,
                        ingestionService,
                        dedupService,
                        stats,
                        stoppingToken);
                }
            }

            if (buffer.Count > 0)
            {
                stats.IngestedCount += await FlushBufferedWindowAsync(
                    buffer,
                    ingestionService,
                    dedupService,
                    stats,
                    stoppingToken);
            }

            stats.ProcessedAtUtc = DateTimeOffset.UtcNow;

            var summaryJson = JsonSerializer.Serialize(stats, _serializerOptions);
            await File.WriteAllTextAsync(summaryFilePath, summaryJson, Encoding.UTF8, stoppingToken);

            _logger.LogInformation(
                "Ingestion completed. Total={Total}, Ingested={Ingested}, Duplicate={Duplicate}, Invalid={Invalid}, Empty={Empty}, Summary={Summary}",
                stats.TotalLinesRead,
                stats.IngestedCount,
                stats.DuplicateCount,
                stats.InvalidCount,
                stats.EmptyLineCount,
                summaryFilePath);
        }

        private static async Task<long> FlushBufferedWindowAsync(
            List<BufferedReading> buffer,
            ISensorReadingIngestionService ingestionService,
            IDeduplicationService dedupService,
            IngestionSummary stats,
            CancellationToken ct)
        {
            var ordered = buffer
                .OrderBy(x => x.Dto.Timestamp)
                .ThenBy(x => x.Dto.Sequence)
                .ToList();

            long count = 0;

            foreach (var item in ordered)
            {
                if (await dedupService.IsDuplicateAsync(item.Key, ct))
                {
                    stats.DuplicateCount++;
                    continue;
                }

                await ingestionService.IngestAsync(item.Dto, ct);
                await dedupService.MarkAsProcessedAsync(item.Key, ct);
                count++;
            }

            buffer.Clear();
            return count;
        }

        private bool TryDeserialize(string rawLine, out IngestSensorReadingDto? dto)
        {
            try
            {
                dto = JsonSerializer.Deserialize<IngestSensorReadingDto>(rawLine, _serializerOptions);
                return dto is not null;
            }
            catch
            {
                dto = null;
                return false;
            }
        }

        private static bool TryValidate(IngestSensorReadingDto dto, out string? error)
        {
            if (string.IsNullOrWhiteSpace(dto.DeviceId))
            {
                error = "DeviceIdIsRequired";
                return false;
            }

            if (dto.Timestamp == default)
            {
                error = "TimestampIsRequired";
                return false;
            }

            if (dto.Sequence < 0)
            {
                error = "SequenceMustBeGreaterThanOrEqualToZero";
                return false;
            }

            error = null;
            return true;
        }

        private static string ResolveFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = "readings.jsonl";

            return Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppContext.BaseDirectory, path);
        }

        private static string ResolveOutputDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = "output";

            return Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppContext.BaseDirectory, path);
        }

        private sealed record BufferedReading(IngestSensorReadingDto Dto, SensorReadingKey Key);

        private sealed class IngestionSummary
        {
            public string? InputFilePath { get; set; }
            public string? OutputDirectory { get; set; }
            public long TotalLinesRead { get; set; }
            public long IngestedCount { get; set; }
            public long DuplicateCount { get; set; }
            public long InvalidCount { get; set; }
            public long EmptyLineCount { get; set; }
            public DateTimeOffset ProcessedAtUtc { get; set; }
        }

        public sealed class WorkerOptions
        {
            public string ReadingsFilePath { get; set; } = "readings.jsonl";
            public int WindowSize { get; set; } = 256;
            public string OutputDirectory { get; set; } = "output";
            public string SummaryFileName { get; set; } = "ingestion-summary.json";
        }
    }
}
