using Kafka.Transfer.App.DataSources.Kafka;
using Kafka.Transfer.App.DataTarget.Kafka;
using Confluent.Kafka;
using Kafka.Transfer.App.DataTarget;
using Microsoft.Extensions.Logging;

namespace Kafka.Transfer.App.Processors;

public class KafkaToKafkaProcessor : BaseProcessor<ConsumeResult<string, string>>
{
    public KafkaToKafkaProcessor(
        KafkaConsumerSource source,
        KafkaTarget target,
        DataTargetTaskChainer taskChainer,
        ILoggerFactory loggerFactory,
        CancellationTokenSource cancellationTokenSource)
        : base(source,
               target,
               taskChainer,
               loggerFactory,
               cancellationTokenSource,
               sourceBoundedCapacity: 1000,
               targetBoundedCapacity: 1000)
    {
    }
}