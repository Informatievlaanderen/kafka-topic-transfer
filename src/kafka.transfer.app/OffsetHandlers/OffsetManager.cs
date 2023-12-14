using Confluent.Kafka;
using MessagePack;

namespace Kafka.Transfer.App.OffsetHandlers;

public class OffsetManager<T> : IOffsetManager<T>
{
    private readonly Action<T> _handleOffset;
    private long _currentOffset;

    public OffsetManager(Action<T> handleOffset)
    {
        _handleOffset = handleOffset;
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(T rawMessage)
    {
        _handleOffset(rawMessage);
    }
}