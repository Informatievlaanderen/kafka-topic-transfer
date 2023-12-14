using Confluent.Kafka;

namespace Kafka.Transfer.App.DataTarget;

public interface IDataTarget: IDisposable
{
    public Task Publish(ConsumeResult<string,string> rawMessage, CancellationToken cancellationToken);
}