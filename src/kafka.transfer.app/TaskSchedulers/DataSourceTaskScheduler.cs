using Microsoft.Extensions.Logging;

namespace Kafka.Transfer.App.TaskSchedulers;

public class DataSourceTaskScheduler : TaskScheduler
{
    private readonly SemaphoreSlim _semaphore;
    private readonly int _taskThreshold;
    private int _tasksProcessed;

    public DataSourceTaskScheduler(
        int maxDegreeOfParallelism, 
        int taskThreshold)
    {
        if (maxDegreeOfParallelism < 1 || taskThreshold < 1)
            throw new ArgumentOutOfRangeException();

        MaximumConcurrencyLevel = maxDegreeOfParallelism;
        _semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
        _taskThreshold = taskThreshold;
        _tasksProcessed = 0;
    }

    protected override IEnumerable<Task>? GetScheduledTasks()
        => Enumerable.Empty<Task>();

    protected override void QueueTask(Task task)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            _semaphore.Wait();
            try
            {
                TryExecuteTask(task);
            }
            finally
            {
                _semaphore.Release();
                TaskProcessed();
            }
        });
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;

    public override int MaximumConcurrencyLevel { get; }

    private void TaskProcessed()
    {
        Interlocked.Increment(ref _tasksProcessed);
        if (_tasksProcessed % _taskThreshold == 0)
        {
            // Introduce a delay or other processing after every threshold
            // Console.WriteLine($"Waiting after processing {} tasks");
            Thread.Sleep(5); // Example: Introduce a 5 millisecond delay
        }
    }
}