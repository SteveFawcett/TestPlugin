using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using CyberDog.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;
using TestDataPlugin.Properties;

namespace TestPlugin.Forms
{
    public partial class TestPage : UserControl, IInfoPage
    {
        private readonly IConfiguration? configuration;
        private readonly ILogger<Test>? logger;
        private ListPanel<DataSet>? listPanel;

        public event EventHandler<CommandItem>? CommandIssued;
        public event EventHandler<bool>? UpdateValueChanged;

        public TestPage()
        {
            InitializeComponent();
            InitializeTestPage();
        }

        public TestPage(IConfiguration configuration, ILogger<Test> logger)
        {
            this.configuration = configuration;
            this.logger = logger;

            InitializeComponent();
            InitializeTestPage();
        }

        private void InitializeTestPage()
        {
            listPanel = new ListPanel<DataSet>
            {
                Location = new Point(5, 110),
                Size = new Size(380, 400)
            };
            Controls.Add(listPanel);

            DisplayIcon();
            PopulateCmdType();
            SetCommands();

            cmdType.SelectedIndexChanged += (s, e) =>
            {
                SetCommands();
            };

            valueUpdater.CheckedChanged += ValueUpdater_CheckedChanged;
        }

        private void SetCommands()
        {
            CommandTypes type = (CommandTypes)Enum.Parse(typeof(CommandTypes), cmdType.SelectedItem?.ToString() ?? "Unknown");

            PopulateTests(type);

        }   
        private void PopulateCmdType()
        {
            cmdType.Items.Clear();
            foreach (var type in Enum.GetValues(typeof(CommandTypes)))
            {
                cmdType.Items.Add(type);
            }
            if (cmdType.Items.Count > 0) cmdType.SelectedIndex = 0;
        }

        private void ValueUpdater_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender == null) return;

            CheckBox chk = (CheckBox)sender;

            UpdateValueChanged?.Invoke(this, chk.Checked);
        }

        public Control GetControl()
        {
            return this;
        }

        public void DisplayIcon()
        {
            pictureBox1.Image = Resources.green;
        }

        public void PopulateTests(CommandTypes selectedType)
        {
            comboBox1.Items.Clear();

            logger?.LogInformation("Populating Commands for Type: {Type}", selectedType.ToString() );

            if ( selectedType == CommandTypes.OperatingSystem )
            {
                foreach (var test in configuration?.GetSection("OsCommand").GetChildren() ?? [])
                {
                    comboBox1.Items.Add(test.Value ?? "Unknown");
                }
            }
            else if( selectedType == CommandTypes.Simulator )
            {
                foreach (var test in configuration?.GetSection("SimCommand").GetChildren() ?? [])
                {
                    comboBox1.Items.Add(test.Value ?? "Unknown");
                }
            }

            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;
        }

        internal void UpdateCards(DataSet dataSet)
        {
            listPanel?.AddUpdateItem(dataSet);
        }

        private void RunCommand(object sender, EventArgs e)
        {
            if(comboBox1.SelectedItem is null) return;
            var cmd = comboBox1.SelectedItem.ToString();

            if ( string.IsNullOrEmpty( cmd )) return;

            CommandTypes type = (CommandTypes)Enum.Parse(typeof(CommandTypes), cmdType.SelectedItem?.ToString() ?? "Unknown");

            logger?.LogInformation("Running Command: {CommandType} - {Command}", type,  cmd );

            var execcmd = new CommandItem()
            {
                Value = cmd,
                Status = CommandStatus.New,
                CommandType = type, 
            };

            CommandIssued?.Invoke( this , execcmd );

        }
    }
}
