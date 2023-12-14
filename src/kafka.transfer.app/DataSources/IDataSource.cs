namespace Kafka.Transfer.App.DataSources;

public interface IDataSource<T> : IDisposable
{
    public IEnumerable<DataRecord<T,byte[]>> Iter(CancellationToken cancellationToken);

    public void HandleOffset(T offset);
}