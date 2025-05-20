using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Soenneker.Enums.DeployEnvironment;
using Soenneker.Extensions.LoggerConfiguration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.GitHub.Runners.OpenApiClient;

public class Program
{
    private static string? _environment;

    private static CancellationTokenSource? _cts;

    private static RunOptions _runOptions;

    public static async Task Main(string[] args)
    {
        if (args.Length == 0 || !int.TryParse(args[0], out int inputCode))
            throw new ArgumentException("A valid integer argument must be passed to the application.");

        // Now you can use inputCode throughout the program
        Console.WriteLine($"Received integer argument: {inputCode}");

        _runOptions = new RunOptions { Code = inputCode };

        _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (string.IsNullOrWhiteSpace(_environment))
            throw new Exception("ASPNETCORE_ENVIRONMENT is not set");

        // Declare CancellationTokenSource in a broader scope
        _cts = new CancellationTokenSource(); // Use 'using' to ensure proper disposal
        Console.CancelKeyPress += OnCancelKeyPress;

        try
        {
            await CreateHostBuilder(args).RunConsoleAsync(_cts.Token);
        }
        catch (Exception e)
        {
            Log.Error(e, "Stopped program because of exception");
            throw;
        }
        finally
        {
            Console.CancelKeyPress -= OnCancelKeyPress; // Detach the handler

            _cts.Dispose();
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Used for WebApplicationFactory, cannot delete, cannot change access, cannot change number of parameters.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        DeployEnvironment envEnum = DeployEnvironment.FromName(_environment);

        LoggerConfigurationExtension.BuildBootstrapLoggerAndSetGlobally(envEnum);

        IHostBuilder? host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                builder.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);

                builder.Build();
            })
            .UseSerilog()
            .ConfigureServices((_, services) => {
                services.AddSingleton(_runOptions);
                Startup.ConfigureServices(services); });

        return host;
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
    {
        eventArgs.Cancel = true; // Prevents immediate termination
        _cts?.Cancel();
    }
}
