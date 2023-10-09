using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace MDRG_Analyzer
{
    public partial class Form1 : Form
    {
        string fileContent;
        JObject saveFileJson;
        string __version__ = "0.1.0";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        public void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set properties of the OpenFileDialog
            openFileDialog.Filter = "MDRG Files (*.mdrg)|*.mdrg|All Files (*.*)|*.*";
            openFileDialog.Title = "Select a .mdrg File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file's path
                string filePath = openFileDialog.FileName;

                // Now you can perform actions based on the selected file
                // For example, you can read the content of the file
                fileContent = File.ReadAllText(filePath);
                saveFileJson = JObject.Parse(fileContent);

                debugTextBox.Text = fileContent;
                JObject savedataObject = JObject.Parse(saveFileJson["saves"][0]["savedata"].ToString());
                string botName = savedataObject["botName"].ToString();
                int saveSlot = (int)saveFileJson["saves"][0]["slot"];
                string moneyVal = savedataObject["money"].ToString();
                int maxCumVal = (int)savedataObject["_maxCum"];
                saveSlotBox.Text = $"{saveSlot}";
                botNameBox.Text = $"{botName}";
                moneyTextBox.Text = $"{moneyVal}";
                maxCumBox.Text = $"{maxCumVal}";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void botNameBox_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }
    }
}
