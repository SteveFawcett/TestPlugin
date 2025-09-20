using CyberDog.Controls;

namespace TestPlugin.Forms
{
    partial class TestPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            label1 = new Label();
            listPanel = new ListPanel<DataSet>();
            comboBox1 = new ComboBox();
            Execute = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(6, 5);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(100, 100);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(113, 35);
            label1.Name = "label1";
            label1.Size = new Size(197, 30);
            label1.TabIndex = 1;
            label1.Text = "Test Data Generator";
            // 
            // listPanel
            // 
            listPanel.BorderStyle = BorderStyle.None;
            listPanel.Location = new Point(6, 111);
            listPanel.Name = "listPanel";
            listPanel.Size = new Size(350, 330);
            listPanel.BackColor = Color.White;
            listPanel.TabIndex = 2;
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(385, 111);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(239, 23);
            comboBox1.TabIndex = 3;
            // 
            // Execute
            // 
            Execute.Location = new Point(630, 111);
            Execute.Name = "Execute";
            Execute.Size = new Size(60, 23);
            Execute.TabIndex = 4;
            Execute.Text = "Test";
            Execute.UseVisualStyleBackColor = true;
            // 
            // TestPage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Execute);
            Controls.Add(comboBox1);
            Controls.Add(listPanel);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Name = "TestPage";
            Size = new Size(721, 456);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Label label1;
        private ListPanel<DataSet> listPanel;
        private ComboBox comboBox1;
        private Button Execute;
    }
}
