namespace Kafka.Transfer.App.DataSources.Kafka;

using System.Linq;
using Confluent.Kafka;

internal static class ConsumerConfigExtensions
{
    public static IConsumer<string, string> BuildConsumer(this ConsumerConfig config, KafkaConsumerSourceOptions sourceOptions)
    {
        var consumerBuilder = new ConsumerBuilder<string, string>(config)
            .SetValueDeserializer(Deserializers.Utf8);
        
        if (sourceOptions.Offset.HasValue)
        {
            consumerBuilder.SetPartitionsAssignedHandler((_, topicPartitions) =>
            {
                var partitionOffset = topicPartitions.Select(x => new TopicPartitionOffset(x.Topic, x.Partition, new Confluent.Kafka.Offset(sourceOptions.Offset.Value)));
                return partitionOffset;
            });
        }

        return consumerBuilder.Build();
    }
}