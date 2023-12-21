using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace Kafka.Transfer.App.DataTarget.Kafka;

public sealed class KafkaTarget : IDataTarget
{
    private bool disposed = false;
    private readonly HttpClient _httpClient;
    private readonly KafkaTargetOptions _options;
    private readonly IProducer<string, string> _producer;
    private readonly DateTime? _lastMessageConsumed;
    private readonly ILogger<KafkaTarget> _logger;

    public KafkaTarget(
        IOptions<KafkaTargetOptions> targetOptions,
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KafkaTarget>();
        _lastMessageConsumed = DateTime.UtcNow;
        _httpClient = new HttpClient();
        _options = targetOptions.Value;
        
        var producerConfig = _options.CreateProduceConfig();
        _producer = new ProducerBuilder<string, string>(producerConfig)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(Serializers.Utf8)
            .Build();
    }

    public async Task Publish(ConsumeResult<string, string> rawMessage, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        try
        {
            var message = rawMessage.Message.Value;
            var key = rawMessage.Message.Key;

            var partition = _options.UseSinglePartition ? new Partition(0) : Partition.Any;
            await _producer.ProduceAsync(
                new TopicPartition(_options.Topic, partition),
                new Message<string, string> { Key = key, Value = message },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e.Message, e);
            throw;
        }
    }
    
    
    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            _httpClient?.Dispose();
            _producer?.Dispose();
        }

        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~KafkaTarget()
    {
        Dispose(false);
    }
}