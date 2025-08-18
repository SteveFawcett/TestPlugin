using System.Diagnostics;
using System.Timers;
using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using TestDataPlugin.Properties;
using Timer = System.Timers.Timer;

namespace TestDataPlugin;

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

    public Test(IConfiguration configuration) :
        base(configuration, null, s_icon, "Local Test", "Test", "Test Data Provider")
    {
        Debug.WriteLine("Starting Test Plugin");
        myTimer.Elapsed += OnTimedEvent;
        myTimer.Enabled = true; // Starts the timer
        Debug.WriteLine("Timer Started");

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
            send.Add(dataSet.Key, dataSet.Value.ToString());
        }

        DataReceived?.Invoke(this, send);
    }
}