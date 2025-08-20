using BroadcastPluginSDK.abstracts;
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

    private readonly List<DataSet> dataSets = [];
    private readonly ILogger<IPlugin> _logger;

    public Test(IConfiguration configuration , ILogger<IPlugin> logger) :
        base(configuration, null, s_icon, "Test")
    {
        _logger = logger;
        _logger.LogInformation("Starting Test Plugin");
        myTimer.Elapsed += OnTimedEvent;
        myTimer.Enabled = true; // Starts the timer

        foreach (var config in configuration.GetSection("Test").GetChildren())
            if (config.Key == "TestData")
                foreach (var dataSet in config.GetChildren())
                    if (!string.IsNullOrEmpty(dataSet["variable"]))
                    {
                        DataSet ds = new()
                        {
                            Key = dataSet["variable"] ?? "Dummy",
                            Maximum = int.Parse(dataSet["Maximum"] ?? "1000"),
                            Minimum = int.Parse(dataSet["Minimum"] ?? "0"),
                            Increment = int.Parse(dataSet["Increment"] ?? "100")
                        };
                        dataSets.Add(ds);
                    }
    }

    public event EventHandler<Dictionary<string, string>>? DataReceived;

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        Dictionary<string, string> send = [];
        foreach (var dataSet in dataSets)
        {
            dataSet.Increase();
            _logger.LogDebug( $"Sending Message {dataSet.Key} => {dataSet.Value.ToString()}");
            send.Add(dataSet.Key, dataSet.Value.ToString());
        }

        DataReceived?.Invoke(this, send);
    }
}