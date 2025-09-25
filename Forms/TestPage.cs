using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;
using TestDataPlugin.Properties;

namespace TestPlugin.Forms
{
    public partial class TestPage : UserControl, IInfoPage
    {
        private IConfiguration? configuration;
        private ILogger<IPlugin>? logger;

        public event EventHandler<CommandItem>? CommandIssued;

        public TestPage()
        {
            InitializeComponent();
        }

        public TestPage(IConfiguration configuration, ILogger<IPlugin> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            InitializeComponent();
            DisplayIcon();

            PopulateTests();
        }

        public Control GetControl()
        {
            return this;
        }

        public void DisplayIcon()
        {
            pictureBox1.Image = Resources.green;
        }

        public void PopulateTests()
        {
            comboBox1.Items.Clear();

            foreach (var test in configuration?.GetSection("Command").GetChildren() ?? [])
            {
                comboBox1.Items.Add(test.Value ?? "Unknown");
            }

            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;
        }

        internal void UpdateCards(DataSet dataSet)
        {
            listPanel.AddUpdateItem(dataSet);
        }

        private void RunCommand(object sender, EventArgs e)
        {
            if(comboBox1.SelectedItem is null) return;
            var cmd = comboBox1.SelectedItem.ToString();

            if ( string.IsNullOrEmpty( cmd )) return;  

            logger?.LogInformation("Running Command: {Command}", cmd );

            var execcmd = new CommandItem()
            {
                Value = cmd,
                Status = CommandStatus.New,
            };

            CommandIssued?.Invoke( this , execcmd );

        }
    }
}
