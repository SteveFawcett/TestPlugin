using PluginBase;
using System.Diagnostics;
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

        public void increase()
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
    class Test : BroadcastPlugin , IProvider
    {
        private List<DataSet> dataSets = new List<DataSet>()
                 { new DataSet() { key = "AIRCRAFT_ALTITUDE" }
        };

        private static Timer? myTimer;

        public Test() : base()
        {
            Name = "Local Test";
            Icon = Properties.Resources.green;
            Description = "Test Data Provider";

            myTimer = new Timer(1000); // 1 second interval
            myTimer.Elapsed += OnTimedEvent;
            myTimer.AutoReset = true;
            myTimer.Enabled = false; 
        }

        public override void Start()
        {
            myTimer.Enabled = true; // Starts the timer
        }

        private void OnTimedEvent(object? sender, System.Timers.ElapsedEventArgs e)
        {
            PluginData send = new();
            Debug.WriteLine($"Sending test data from {Name} plugin");
            foreach (DataSet dataSet in dataSets)
            {
                dataSet.increase();
                send.Add( dataSet.key , dataSet.value.ToString());
            }
            DataReceived?.Invoke(this, send);
        }

        public event EventHandler<PluginData>? DataReceived;
    }
}

