using BroadcastPluginSDK;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Timer = System.Timers.Timer;

namespace TestDataPlugin
{
    internal class DataSet
    {
        public string Key = string.Empty;
        public int Value = 0;
        public int Maximum = 1000;
        public int Minimum = 0;
        public int Increment = 100;

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
        private readonly List<DataSet> dataSets = [];
        private static readonly Image s_icon = Properties.Resources.green;

        private static readonly Timer myTimer = new(1000)
        {
            Enabled = false,
            AutoReset = true
        };

        public Test(IConfiguration configuration) : 
            base(configuration, null, s_icon, "Local Test", "Test", "Test Data Provider")
        {
            Debug.WriteLine("Starting Test Plugin");
            myTimer.Elapsed += OnTimedEvent;
            myTimer.Enabled = true; // Starts the timer
            Debug.WriteLine("Timer Started");

            foreach (IConfigurationSection config in configuration.GetSection( "Test").GetChildren())
            {
                if (config.Key == "TestData")
                {
                    foreach (IConfigurationSection dataSet in config.GetChildren())
                    {
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
                }
            }
        }

        private void OnTimedEvent(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dictionary<string, string> send = [];
            foreach (DataSet dataSet in dataSets)
            {
                dataSet.Increase();
                send.Add(dataSet.Key, dataSet.Value.ToString());
            }

            DataReceived?.Invoke(this, send);
        }

        public event EventHandler<Dictionary<string, string>>? DataReceived;
    }
}

