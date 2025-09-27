using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using CyberDog.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Timers;
using TestDataPlugin.Properties;
using TestPlugin.Forms;
using Timer = System.Timers.Timer;

namespace TestPlugin;

internal class DataSet : IUpdatableItem
{
    public int Increment = 100;
    public int Maximum = 1000;
    public int Minimum = 0;

    public bool Update = true;

    private int _value = 0;
    public string Value { get => _value.ToString();
                                  set => _value = int.Parse(value); }    
    public string Key { set; get; } = "Dummy";
    public void Increase()
    {
        if (!Update) return;

        _value += Increment;
        if (_value > Maximum)
        {
            _value = Maximum;
            Increment = -Increment;
        }

        if (_value < Minimum)
        {
            _value = Minimum;
            Increment = -Increment;
        }
    }
}

internal class Test : BroadcastPluginBase, IProvider
{
    private const string STANZA = "Test";

    private static readonly Image s_icon = Resources.green;

    private static readonly Timer myTimer = new(1000)
    {
        Enabled = false,
        AutoReset = true
    };


    private readonly List<DataSet> dataSets = [];
    private readonly ILogger<IPlugin>? _logger;

    public static TestPage? _infoPage;

    public Test() : base() { }

    public Test(IConfiguration configuration , ILogger<IPlugin> logger , IPluginRegistry pluginRegistry) :
        base(configuration, DisplayPage( configuration , logger  , pluginRegistry ), s_icon, STANZA )
    {
        _logger = logger;
        _logger?.LogInformation("Starting Test Plugin");
        myTimer.Elapsed += OnTimedEvent;
        myTimer.Enabled = true; // Starts the timer

        foreach (var config in configuration.GetSection( STANZA ).GetChildren())
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

    }

    public static TestPage DisplayPage(IConfiguration configuration, ILogger<IPlugin> logger , IPluginRegistry pluginRegistry)
    {

        _infoPage = new TestPage(configuration.GetSection( STANZA ), logger);

        _infoPage.CommandIssued += (s, e) =>
        {
            ICache? cache = pluginRegistry.MasterCache();
            cache?.CommandWriter(e);
        };

        return _infoPage;
    }

    public event EventHandler<CacheData>? DataReceived;

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        var send = dataSets
            .Select(dataSet =>
            {
                _logger?.LogDebug("Updating {key} to {value}", dataSet.Key, dataSet.Value);
                dataSet.Increase();
                _infoPage?.UpdateCards(dataSet);
                return new { dataSet.Key, Value = dataSet.Value.ToString() };
            })
            .ToDictionary(x => x.Key, x => x.Value);

        
        DataReceived?.Invoke(this, new CacheData() {  Data = send , Prefix = CachePrefixes.DATA });
    }
}