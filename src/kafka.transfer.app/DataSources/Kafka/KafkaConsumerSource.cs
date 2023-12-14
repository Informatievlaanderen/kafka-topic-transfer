using Confluent.Kafka;
using MessagePack;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Kafka.Transfer.App.DataSources.Kafka;

public sealed class KafkaConsumerSource : IDataSource<ConsumeResult<string, string>>
{
    private bool disposed = false;
    private readonly KafkaConsumerSourceOptions kafkaConsumerSourceOptions;
    private readonly IConsumer<string, string> _consumer;
    private long lastOffsetValue = -1001L;

    public KafkaConsumerSource(IOptions<KafkaConsumerSourceOptions> kafkaConsumerSourceOptions)
    {
        this.kafkaConsumerSourceOptions = kafkaConsumerSourceOptions.Value;
        _consumer = this.kafkaConsumerSourceOptions.CreateConsumerConfig()
            .BuildConsumer(this.kafkaConsumerSourceOptions);
    }

    public IEnumerable<DataRecord<ConsumeResult<string, string>, byte[]>> Iter(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(kafkaConsumerSourceOptions.Topic);
        Console.WriteLine($"Subscribed to {kafkaConsumerSourceOptions.Topic}");
        while (!cancellationToken.IsCancellationRequested)
        {
            ConsumeResult<string, string> consumeResult;
            try
            {
                consumeResult = _consumer.Consume(cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("Something went wrong...");
                Console.WriteLine(ex.Message);
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
            // Console.WriteLine($"Offset {consumeResult.Offset.Value} posted.");
            // if (consumeResult.Offset.Value % kafkaConsumerSourceOptions.PauseTriggerMessageCount == 0)
            // {
            //     //wait one sec per thousand
            //     Thread.Sleep(kafkaConsumerSourceOptions.PauseTriggerInterval);
            // } 
            yield return dataRecord;
        }

        Console.WriteLine("Unsubscribing...");
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