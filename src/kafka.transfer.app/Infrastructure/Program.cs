using Kafka.Transfer.App.DataSources.Kafka;
using Kafka.Transfer.App.DataTarget;
using Kafka.Transfer.App.DataTarget.Kafka;
using Kafka.Transfer.App.Processors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;

namespace Kafka.Transfer.App.Infrastructure;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                var env = hostingContext.HostingEnvironment;
                var a = env.ContentRootPath;
                builder
                    .SetBasePath(env.ContentRootPath)
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{env.EnvironmentName.ToLowerInvariant()}.json", optional: true,
                        reloadOnChange: false)
                    .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true,
                        reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);
            }).ConfigureLogging((hostContext, builder) =>
            {
                SelfLog.Enable(Console.WriteLine);

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(hostContext.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithEnvironmentUserName()
                    //.Destructure.JsonNetTypes()
                    .CreateLogger();

                builder.ClearProviders();
                builder.AddSerilog(Log.Logger);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .AddSingleton(i => new CancellationTokenSource())
                    .AddSingleton<DataTargetTaskChainer>()
                    .Configure<KafkaConsumerSourceOptions>(
                        hostContext.Configuration.GetSection(nameof(KafkaConsumerSourceOptions)))
                    .Configure<KafkaTargetOptions>(
                        hostContext.Configuration.GetSection(nameof(KafkaTargetOptions)))
                    .AddScoped<KafkaConsumerSource>()
                    .AddScoped<KafkaTarget>()
                    .AddHostedService<KafkaToKafkaProcessor>();
            })
            .UseConsoleLifetime()
            .Build();


        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        try
        {
            var cancellationToken = host.Services.GetRequiredService<CancellationTokenSource>();
            var task = host.RunAsync(cancellationToken.Token);
            logger.LogInformation("Starting kafka.topic.transfer");
            Console.WriteLine("Press any key to exist");
            Console.ReadKey(true);
            cancellationToken.Cancel();
            Console.WriteLine("Shutdown requested!");
            await task;
            while (task.Status != TaskStatus.RanToCompletion)
            {
                await Task.Delay(300);
            }

            logger.LogInformation("Bye bye!");
        }
        catch (AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.InnerExceptions)
            {
                logger.LogCritical(innerException, "Encountered a fatal exception, exiting program.");
            }
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
            Log.CloseAndFlush();
            // Allow some time for flushing before shutdown.
            await Task.Delay(500, default);
            throw;
        }
        finally
        {
            logger.LogWarning("Stopping...");
        }
    }
}