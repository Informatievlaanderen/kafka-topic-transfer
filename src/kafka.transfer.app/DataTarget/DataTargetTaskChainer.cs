using Microsoft.Extensions.Logging;

namespace Kafka.Transfer.App.DataTarget;

public class DataTargetTaskChainer
{
    private long _currentOffset = -5;
    private Task? _currentTask = null;
    private readonly ILogger<DataTargetTaskChainer> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public DataTargetTaskChainer(ILoggerFactory loggerFactory, CancellationTokenSource cancellationTokenSource)
    {
        _logger = loggerFactory.CreateLogger<DataTargetTaskChainer>();
        _cancellationTokenSource = cancellationTokenSource;
    }
    
    public void AddToChain(long offset, Task task)
    {
        // Initial set
        if (_currentOffset == -5 &&
            offset == 0L)
        {
            _currentTask = task;
            _currentOffset = offset;
            _currentTask.ContinueWith(t =>
            {
                _logger.LogCritical("Failed to publish message with offset {CurrentOffset}", _currentOffset);
                _cancellationTokenSource.Cancel();
            }, TaskContinuationOptions.OnlyOnFaulted);
            return;
        }

        // Error
        if (_currentOffset >= offset)
        {
            _logger.LogCritical("Current offset is higher then received offset");
            _cancellationTokenSource.Cancel();
            return;
        }

        //FIFO: wait for current Offset catchup before chaining
        long expectedNextOffset = _currentOffset + 1;
        if (offset > expectedNextOffset)
        {
            while (true)
            {
                expectedNextOffset = _currentOffset + 1;
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
                if (expectedNextOffset == offset)
                {
                    break;
                }
            }
        }
        
        _currentTask!.ContinueWith(t => { 
            _currentOffset = offset;
            return task;
        }, TaskContinuationOptions.RunContinuationsAsynchronously);
    }
}