using System.Threading.Tasks.Dataflow;
using Confluent.Kafka;
using Kafka.Transfer.App.OffsetHandlers;
using Microsoft.Extensions.Logging;

namespace Kafka.Transfer.App.DataTarget;

public class DataTargetBlock<T> : ITargetBlock<DataRecord<T,byte[]>>
{
    private readonly IDataTarget _target;
    private readonly BufferBlock<DataRecord<T,byte[]>> _bufferBlock;
    private readonly CancellationTokenSource _cancellationToken;
    private readonly  IOffsetManager<T> _offsetManager;
    private readonly ILogger<DataTargetBlock<T>> _logger;
    private readonly DataTargetTaskChainer _taskChainer;
    
    public DataTargetBlock (
        IDataTarget target,
        int? boundedCapacity,
        IOffsetManager<T> offsetManager,
        DataTargetTaskChainer taskChainer,
        ILoggerFactory loggerFactory,
        CancellationTokenSource cancellationToken)
    {
        _target = target;
        _offsetManager = offsetManager;
        _taskChainer = taskChainer;
        _cancellationToken = cancellationToken;
        _bufferBlock = new BufferBlock<DataRecord<T,byte[]>>(new ExecutionDataflowBlockOptions()
        {
            BoundedCapacity = boundedCapacity ?? 1000,
            CancellationToken = cancellationToken.Token,
        });
        _logger = loggerFactory.CreateLogger<DataTargetBlock<T>>();
    }

    public async Task Start()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            var block = await _bufferBlock.ReceiveAsync(_cancellationToken.Token);
            var rawMessage = block.RawMessage as ConsumeResult<string, string>;
            
            //Observer sent -> 
            var offset = rawMessage!.TopicPartitionOffset.Offset.Value;
            var logLevel = offset % 1000L == 0 ? LogLevel.Information : LogLevel.Debug;
            _logger.Log(logLevel, $"Publishing consumed message with offset {offset}");

            var t = _target.Publish(rawMessage, _cancellationToken.Token)
                .ContinueWith(t => _offsetManager.OnNext(block.RawMessage),
                    TaskContinuationOptions.ExecuteSynchronously);
            _taskChainer.AddToChain(offset, t);
            
            //No more messages in the queue
            if (_bufferBlock.Count == 0)
            {
                _logger.LogInformation("All messages in TPL queue are consumed and published. \nWaiting for messages from consumer.");
                _logger.LogInformation($"Current offset: {offset}");
            }
        }
    }

    public DataflowMessageStatus OfferMessage(
        DataflowMessageHeader messageHeader,
        DataRecord<T,byte[]> messageValue,
        ISourceBlock<DataRecord<T,byte[]>>? source,
        bool consumeToAccept)
    {
        var bufferBlock = (ITargetBlock<DataRecord<T,byte[]>>)_bufferBlock;
        var status = bufferBlock.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        return status;
    }

    public void Complete()
    {
        _target?.Dispose();
        _bufferBlock.Complete();
        _cancellationToken.Cancel();
    }

    public void Fault(Exception exception)
    {
        _logger.LogCritical("Error in target block: {Exception}", exception);
        var bufferBlock = (ITargetBlock<DataRecord<T,byte[]>>)_bufferBlock;
        bufferBlock.Fault(exception);
    }

    public Task Completion
    {
        get
        {
            _target?.Dispose();
            return _bufferBlock.Completion;
        }
    }
}