using BroadcastPluginSDK;
using System.Diagnostics;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Timer = System.Timers.Timer;

namespace TestDataPlugin
{
    internal class DataSet
    {
        public string key = string.Empty;
        public int value = 0;
        public int maximum = 1000;
        public int minimum = 0;
        public int increment = 100;

        public void Increase()
        {
            value += increment;
            if (value > maximum)
            {
                value = maximum;
                increment = -increment;
            }

            if (value < minimum)
            {
                value = minimum;
                increment = -increment;
            }
        }
    }
    internal class Test : BroadcastPluginBase, IProvider
    {
        private readonly List<DataSet> dataSets = [];
        public override string Stanza => "Test";

        private static readonly Timer myTimer = new(1000)
        {
            Enabled = false,
            AutoReset = true
        };

        public Test() : base()
        {
            Name = "Local Test";
            Icon = Properties.Resources.green;
            Description = "Test Data Provider";
            myTimer.Elapsed += OnTimedEvent;
        }

        public override string Start()
        {
            myTimer.Enabled = true; // Starts the timer
            Logger.Log(Name, $"Plugin initialized");

            if (Configuration == null)
            {
                Logger.Log(Name, "Configuration is null");
                return string.Empty;
            }

            foreach (Microsoft.Extensions.Configuration.IConfigurationSection config in Configuration.GetChildren())
            {
                if (config.Key == "TestData")
                {
                    foreach (Microsoft.Extensions.Configuration.IConfigurationSection dataSet in config.GetChildren())
                    {
                        if (!string.IsNullOrEmpty(dataSet["variable"]))
                        {
                            DataSet ds = new()
                            {
                                key = dataSet["variable"] ?? "Dummy",
                                maximum = int.Parse(dataSet["maximum"] ?? "1000"),
                                minimum = int.Parse(dataSet["minimum"] ?? "0"),
                                increment = int.Parse(dataSet["increment"] ?? "100")
                            };
                            dataSets.Add(ds);
                        }
                    }
                }
            }
            return string.Empty;
        }

        private void OnTimedEvent(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dictionary<string, string> send = [];
            Debug.WriteLine($"Timed Event: Sending test data from {Name} plugin");
            foreach (DataSet dataSet in dataSets)
            {
                dataSet.Increase();
                send.Add(dataSet.key, dataSet.value.ToString());
            }

            DataReceived?.Invoke(this, send);
        }

        public event EventHandler<Dictionary<string, string>>? DataReceived;
    }
}

