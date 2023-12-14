using System.Threading.Tasks.Dataflow;
using Kafka.Transfer.App.TaskSchedulers;
using Confluent.Kafka;

namespace Kafka.Transfer.App.DataSources;

public class DataSourceBlock<T> : ISourceBlock<DataRecord<T,byte[]>>
{
    private readonly IDataSource<T> _dataSource;
    private readonly BufferBlock<DataRecord<T,byte[]>> _bufferBlock;
    private readonly CancellationTokenSource _cancellationToken;

    public DataSourceBlock(IDataSource<T> source, int? boundedCapacity, CancellationTokenSource cancellationToken)
    {
        _dataSource = source;
        _cancellationToken = cancellationToken;
        _bufferBlock = new BufferBlock<DataRecord<T,byte[]>>(
            new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = boundedCapacity ?? 1000,
                CancellationToken = cancellationToken.Token,
                TaskScheduler = new DataSourceTaskScheduler(1, 500)
            });
    }

    public async Task Start()
    {
        foreach (var message in _dataSource.Iter(_cancellationToken.Token))
        {
            // 10ms delay till target block is available
            // Note don't exceed max.poll.timeout or else it will get removed from the group
            // A rebalance will occur
            while (!(await _bufferBlock.SendAsync(message)))
            {
                await Task.Delay(10);
            }
            //Console.WriteLine("Source Posted");
        }
    }

    public void Complete()
    {
        _cancellationToken.Cancel();
        _dataSource?.Dispose();
        _bufferBlock.Complete();
    }

    public Task Completion
    {
        get
        {
            _dataSource?.Dispose();
            return _bufferBlock.Completion;
        }
    }

    public void Fault(Exception exception)
    {
        var bufferBlock = (ISourceBlock<DataRecord<T,byte[]>>)_bufferBlock;
        bufferBlock.Fault(exception);
    }

    public DataRecord<T,byte[]>? ConsumeMessage(
        DataflowMessageHeader messageHeader,
        ITargetBlock<DataRecord<T,byte[]>> target,
        out bool messageConsumed)
    {
        var bufferBlock = (ISourceBlock<DataRecord<T,byte[]>>)_bufferBlock;
        return bufferBlock.ConsumeMessage(messageHeader, target, out messageConsumed);
    }

    public IDisposable LinkTo(ITargetBlock<DataRecord<T,byte[]>> target, DataflowLinkOptions linkOptions)
    {
        return _bufferBlock.LinkTo(target, linkOptions);
    }

    public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<DataRecord<T,byte[]>> target)
    {
        var bufferBlock = (ISourceBlock<DataRecord<T,byte[]>>)_bufferBlock;
        bufferBlock.ReleaseReservation(messageHeader, target);
    }

    public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<DataRecord<T,byte[]>> target)
    {
        var bufferBlock = (ISourceBlock<DataRecord<T,byte[]>>)_bufferBlock;
        return bufferBlock.ReserveMessage(messageHeader, target);
    }

    public Action<T> OffsetHandler => _dataSource.HandleOffset;
}