using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Kafka.Transfer.App.DataSources.Kafka;

public sealed class KafkaConsumerSource : IDataSource<ConsumeResult<string, string>>
{
    private bool disposed = false;
    private readonly KafkaConsumerSourceOptions kafkaConsumerSourceOptions;
    private readonly IConsumer<string, string> _consumer;
    private long lastOffsetValue = -1001L;
    private readonly ILogger<KafkaConsumerSource> _logger;

    public KafkaConsumerSource(
        IOptions<KafkaConsumerSourceOptions> kafkaConsumerSourceOptions,
        ILoggerFactory loggerFactory)
    {
        
        _logger = loggerFactory.CreateLogger<KafkaConsumerSource>();
        this.kafkaConsumerSourceOptions = kafkaConsumerSourceOptions.Value;
        _consumer = this.kafkaConsumerSourceOptions.CreateConsumerConfig()
            .BuildConsumer(this.kafkaConsumerSourceOptions);
    }

    public IEnumerable<DataRecord<ConsumeResult<string, string>, byte[]>> Iter(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(kafkaConsumerSourceOptions.Topic);
        _logger.LogInformation($"Subscribed to {kafkaConsumerSourceOptions.Topic}");
        while (!cancellationToken.IsCancellationRequested)
        {
            ConsumeResult<string, string> consumeResult;
            try
            {
                consumeResult = _consumer.Consume(cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogCritical("Something went wrong...\n {ex.Message}", ex);
                yield break;
            }

            if (consumeResult?.Message.Value == null) //if no message is found, returns null
            {
                Thread.Sleep(300);
                continue;
            }

            var dataRecord = new DataRecord<ConsumeResult<string, string>, byte[]>()
            {
                RawMessage = consumeResult,
            };
            yield return dataRecord;
        }

        _logger.LogInformation("Unsubscribing...");
        _consumer.Unsubscribe();
    }

    public void HandleOffset(ConsumeResult<string, string> offset)
    {
        _consumer.StoreOffset(offset);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            _consumer?.Unsubscribe();
            _consumer?.Dispose();
        }

        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~KafkaConsumerSource()
    {
        Dispose(false);
    }
}