using System.Threading.Tasks.Dataflow;
using Kafka.Transfer.App.DataSources;
using Kafka.Transfer.App.DataTarget;
using Kafka.Transfer.App.OffsetHandlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kafka.Transfer.App.Processors;

public abstract class BaseProcessor<T> : BackgroundService
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly DataSourceBlock<T> _source;
    private readonly DataTargetBlock<T> _target;

    protected BaseProcessor(
        IDataSource<T> source,
        IDataTarget target,
        DataTargetTaskChainer taskChainer,
        ILoggerFactory loggerFactory,
        CancellationTokenSource cancellationTokenSource,
        int sourceBoundedCapacity = 1000,
        int targetBoundedCapacity = 1000)
    {
        _cancellationTokenSource = cancellationTokenSource;

        _source = new DataSourceBlock<T>(source, targetBoundedCapacity, _cancellationTokenSource);
        IOffsetManager<T> offsetManager = new OffsetManager<T>(_source.OffsetHandler);
        _target = new DataTargetBlock<T>(target, sourceBoundedCapacity, offsetManager, taskChainer, loggerFactory, _cancellationTokenSource);
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = false };
        _source.LinkTo(_target, linkOptions);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var target= _target.Start().ConfigureAwait(true);
        await Task.Delay(50);
        await _source.Start().ConfigureAwait(false);
        await target;

        _target.Complete();
        _source.Complete();
    }
}