using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestDataPlugin.Properties;

namespace TestPlugin.Forms
{
    public partial class TestPage : UserControl , IInfoPage
    {
        private IConfiguration? configuration;
        private ILogger<IPlugin>? logger;

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

            foreach( var test in configuration?.GetSection("Command").GetChildren() ?? [])
            {
                comboBox1.Items.Add(test.Value ?? "Unknown");
            }

            comboBox1.SelectedIndex = 0;
        }

        internal void UpdateCards( DataSet dataSet )
        {
            listPanel.AddUpdateItem( dataSet );
        }
    }
}
