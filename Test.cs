using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using CyberDog.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Timers;
using TestPlugin.Forms;
using TestPlugin.Properties;
using Timer = System.Timers.Timer;

namespace TestPlugin;

internal class DataSet : IUpdatableItem
{
    public int Increment = 100;
    public int Maximum = 1000;
    public int Minimum = 0;

    public Rectangle ValueRect = Rectangle.Empty;
    public bool Update = false;

    private int _value = 0;
    public string Value { get => _value.ToString();
                                  set => _value = int.Parse(value); }    
    public string Key { set; get; } = "Dummy";
    public void Increase()
    {
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

    private static readonly Timer TempTimer = new(10000)
    {
        Enabled = false,
        AutoReset = true
    };

    private readonly List<DataSet> dataSets = [];
    private readonly ILogger<IPlugin>? _logger;
    private readonly IPluginRegistry? _pluginRegistry;
    private readonly IConfiguration? _configuration;

    public static TestPage? _infoPage;

    public Test() : base() { }

    public Test(IConfiguration configuration , ILogger<IPlugin> logger , IPluginRegistry pluginRegistry) :
        base(configuration, DisplayPage( configuration , logger ), s_icon, STANZA )
    {
        _logger = logger;
        _pluginRegistry = pluginRegistry;
        _configuration = configuration.GetSection(STANZA) ;
        _logger?.LogInformation("Starting Test Plugin");
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

    }

    public static TestPage DisplayPage(IConfiguration configuration, ILogger<IPlugin> logger)
    {
        _infoPage = new TestPage(configuration.GetSection( STANZA ), logger);
        return _infoPage;
    }

    public event EventHandler<Dictionary<string, string>>? DataReceived;

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

        DataReceived?.Invoke(this, send);
    }

    private void OnTempTimedEvent(object? sender, ElapsedEventArgs e)
    {
        if(_pluginRegistry == null) return;

        ICache? cache = _pluginRegistry.MasterCache();
        _logger?.LogDebug("Found Cache: {cache}", cache != null ? "Yes" : "No");

        if( cache == null) return;

        _logger?.LogDebug("Adding Dummy Command");
        cache?.CommandWriter(
            new CommandItem()
            {
                Command = _configuration?.GetValue<string>("Command") ?? "Empty command",
                Parameters = new Dictionary<string, string>() { { "Time", DateTime.Now.ToString("HH:mm:ss") } },
                Status = CommandStatus.New,
            });

        _logger?.LogDebug("Dummy Command Executed");
    }
}