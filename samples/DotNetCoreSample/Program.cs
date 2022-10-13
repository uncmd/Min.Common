using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Min.Common.Reflection;
using Serilog;
using System.Diagnostics;

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Information()
#else
    .MinimumLevel.Warning()
#endif
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = new HostBuilder()
    .UseSerilog()
    .Build();

var loggerFactory = (ILoggerFactory)host.Services.GetService(typeof(ILoggerFactory));
var logger = loggerFactory.CreateLogger(nameof(Program));

var sw = Stopwatch.StartNew();
var t1 = AssemblyHelper.FindAllDependencyAssemblies(logger);
sw.Stop();
Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");

await host.RunAsync();
