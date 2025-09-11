using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Timers;
using TestPlugin.Properties;
using Timer = System.Timers.Timer;

namespace TestPlugin;

internal class DataSet
{
    public int Increment = 100;
    public string Key = string.Empty;
    public int Maximum = 1000;
    public int Minimum;
    public int Value;

    public void Increase()
    {
        Value += Increment;
        if (Value > Maximum)
        {
            Value = Maximum;
            Increment = -Increment;
        }

        if (Value < Minimum)
        {
            Value = Minimum;
            Increment = -Increment;
        }
    }
}

internal class Test : BroadcastPluginBase, IProvider
{
    private static readonly Image s_icon = Resources.green;

    private static readonly Timer myTimer = new(1000)
    {
        Enabled = false,
        AutoReset = true
    };

    private static readonly Timer TempTimer = new(10000)
    {
        Enabled = false,
        AutoReset = true
    };

    private readonly List<DataSet> dataSets = [];
    private readonly ILogger<IPlugin>? _logger;
    private readonly IPluginRegistry? _pluginRegistry;

    public Test() : base() { }

    public Test(IConfiguration configuration , ILogger<IPlugin> logger , IPluginRegistry pluginRegistry) :
        base(configuration, null, s_icon, "Test")
    {
        _logger = logger;
        _pluginRegistry = pluginRegistry;
        _logger.LogInformation("Starting Test Plugin");
        myTimer.Elapsed += OnTimedEvent;
        myTimer.Enabled = true; // Starts the timer
        TempTimer.Elapsed += OnTempTimedEvent;
        TempTimer.Enabled = true; // Starts the timer

        foreach (var config in configuration.GetSection("Test").GetChildren())
            if (config.Key == "TestData")
                foreach (var dataSet in config.GetChildren())
                    if (!string.IsNullOrEmpty(dataSet["variable"]))
                    {
                        DataSet ds = new()
                        {
                            Key = dataSet["variable"] ?? "Dummy",
                            Maximum = int.Parse(dataSet["maximum"] ?? "1000"),
                            Minimum = int.Parse(dataSet["minimum"] ?? "0"),
                            Increment = int.Parse(dataSet["increment"] ?? "100")
                        };
                        dataSets.Add(ds);
                    }

        _pluginRegistry = pluginRegistry;
    }

    public event EventHandler<Dictionary<string, string>>? DataReceived;

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        Dictionary<string, string> send = [];
        foreach (var dataSet in dataSets)
        {
            dataSet.Increase();
            _logger?.LogDebug( $"Sending Message {dataSet.Key} => {dataSet.Value.ToString()}");
            send.Add(dataSet.Key, dataSet.Value.ToString());
        }

        DataReceived?.Invoke(this, send);
 
    }

    private void OnTempTimedEvent(object? sender, ElapsedEventArgs e)
    {
        if(_pluginRegistry == null) return;

        ICache? cache = _pluginRegistry.MasterCache();
        _logger?.LogDebug("Found Cache: {cache}", cache != null ? "Yes" : "No");

        if( cache == null) return;

        _logger?.LogDebug("Getting Commands");

        foreach (var command in cache.CommandReader(CommandStatus.New))
            _logger?.LogDebug("Command: {command} Status: {status}", command.Command.ToString(), command.Status.ToString());
  
        _logger?.LogDebug("Adding Dummy Command");
        cache?.CommandWriter(
            new CommandItem()
            {
                Command = Commands.START_FLIGHT_SIMULATOR,
                Parameters = new Dictionary<string, string>() { { "Time", DateTime.Now.ToString("HH:mm:ss") } },
                Status = CommandStatus.New,
            });

        _logger?.LogDebug("Dummy Command Executed");
    }
}