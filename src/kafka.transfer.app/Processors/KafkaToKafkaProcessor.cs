using Kafka.Transfer.App.DataSources.Kafka;
using Kafka.Transfer.App.DataTarget.Kafka;
using Confluent.Kafka;

namespace Kafka.Transfer.App.Processors;

public class KafkaToKafkaProcessor : BaseProcessor<ConsumeResult<string, string>>
{
    public KafkaToKafkaProcessor(
        KafkaConsumerSource source,
        KafkaTarget target,
        CancellationTokenSource cancellationTokenSource)
        : base(source,
               target,
               cancellationTokenSource,
               sourceBoundedCapacity: 1000,
               targetBoundedCapacity: 1000)
    {
    }
}