using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MDRG_Analyzer
{
    public partial class Form1 : Form
    {
        // Initialize some variables
        string fileContent;
        JObject saveFileJson;
        string __version__ = "1.0.1";
        int selectedSaveFile = -1;
        string repoUrl = "https://github.com/Wehrmachtserdbeere/MDRG-Analyzer";
        public Form1()
        {
            InitializeComponent();
            versionBox.Text = $"{__version__}";
        }

        public static void openWebsite(string x)
        {
            System.Diagnostics.Process.Start("cmd", "/C start" + " " + x);
        }

        private void AddRadioButtons(int numberOfFiles)
        {
            flowLayoutPanel1.Controls.Clear(); // Clear existing controls

            for (int i = 1; i <= numberOfFiles; i++)
            {
                RadioButton radioButton = new RadioButton();
                radioButton.Text = "File " + i;
                radioButton.Name = "radioButton" + i.ToString();
                radioButton.AutoSize = true;

                radioButton.CheckedChanged += new EventHandler(RadioButton_CheckedChanged);

                flowLayoutPanel1.Controls.Add(radioButton);
            }
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton.Checked)
            {
                string buttonText = radioButton.Text;
                string saveNumberString = buttonText.Substring(buttonText.LastIndexOf(' ') + 1);

                if (int.TryParse(saveNumberString, out int saveNumber))
                {
                    selectedSaveFile = saveNumber - 1;
                }
            }
            reloadValues();
        }

        public void loadToolStripMenuItem_Click(object sender, EventArgs e) // On clicking "Load File"
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set properties of the OpenFileDialog to only accept "mdrg" files
            openFileDialog.Filter = "MDRG Files (*.mdrg)|*.mdrg|All Files (*.*)|*.*";
            openFileDialog.Title = "Select a .mdrg File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file's path
                string filePath = openFileDialog.FileName;

                // Load save file into variable, then parse the JSon
                fileContent = File.ReadAllText(filePath);
                saveFileJson = JObject.Parse(fileContent);

                // Deserialize into dynamic object
                dynamic jsonData = JsonConvert.DeserializeObject(fileContent);

                // Extract Saves array
                var saves = jsonData.saves;
                List<int> slots = new List<int>();
                foreach (var save in saves)
                {
                    slots.Add((int)save.slot);
                }
                int[] slotsArray = slots.ToArray(); // Convert List to Array
                int slotsAmount = slotsArray.Length;
                AddRadioButtons(slotsAmount);

                debugTextBox.Text = fileContent; // Pump entire JSon into Debug

                

                /* TODO
                 * Add more stuff and tabs
                 */
            }
        }
        private void reloadValues()
        {
            // Set the variables
            JObject savedataObject = JObject.Parse(saveFileJson["saves"][selectedSaveFile]["savedata"].ToString());
            JObject achievementsObject = JObject.Parse(saveFileJson["achievements"].ToString()); // Achievements

            string botName = savedataObject.Value<string>("botName");
            string moneyVal = savedataObject.Value<string>("money");
            string gameVersion = savedataObject.Value<string>("gameVersion");
            string playerName = savedataObject.Value<string>("playerName");

            int saveSlot = saveFileJson["saves"][selectedSaveFile].Value<int>("slot");
            int casinoTokensVal = savedataObject.Value<int>("casinoTokens");
            int botLust = savedataObject.Value<int>("_lust");
            int botSympathy = savedataObject.Value<int>("_sympathy");
            int botLonging = savedataObject.Value<int>("_longing");
            int botInt = savedataObject.Value<int>("inteligence"); // The grammar error is in the base game.
            int gameStage = savedataObject.Value<int>("stage");
            int subs = savedataObject.Value<int>("subs");
            int followers = savedataObject.Value<int>("followers");
            int totalStreamtime = savedataObject.Value<int>("streamedFor");
            int streamDonationsVal = savedataObject.Value<int>("moneyEarnedFromDonations");
            int longestStreamtime = savedataObject.Value<int>("longestStream");
            int timesCameInsideVal = savedataObject.Value<int>("timesCameInside");
            int timesCameInsideAnalVal = savedataObject.Value<int>("timesCameInsideAnal");
            int timesCameInMouthVal = savedataObject.Value<int>("timesCameInMouth");
            int timesCameOutside = savedataObject.Value<int>("timesCameOutside");
            int ingameTime = saveFileJson["saves"][selectedSaveFile].Value<int>("ingameTime");

            double maxCumVal = savedataObject.Value<double>("_maxCum");
            double currentCumVal = savedataObject.Value<double>("_remainingCum");
            double health = savedataObject.Value<double>("_health");
            double currentStamina = savedataObject.Value<double>("_stamina");
            double mentalHealth = savedataObject.Value<double>("_mentalHealth");
            double botMood = savedataObject.Value<double>("_mood");
            double cumInPussyVal = savedataObject.Value<double>("_cumInside");
            double cumInAssVal = savedataObject.Value<double>("_cumInsideAnal");
            double cumInStomachVal = savedataObject.Value<double>("_cumInsideStomach");
            
            JArray jsVisitedWebsites = (JArray)saveFileJson["visitedWebsites"];
            JArray jsGottenAchievements = (JArray)achievementsObject["values"];
            string[] visitedWebsites = jsVisitedWebsites.ToObject<string[]>();
            string[] gottenAchievements = jsGottenAchievements.ToObject<string[]>();

            // Continue various variables
            bool lightSwitchOn = savedataObject.Value<bool>("lightSwitchOn");

            // Convert raw time into days, hours, and minutes
            int streamDays = totalStreamtime / (24 * 60);
            int streamHours = (totalStreamtime % (24 * 60)) / 60;
            int streamMinutes = totalStreamtime % 60;

            int longestStreamDays = longestStreamtime / (24 * 60);
            int longestStreamHours = (longestStreamtime % (24 * 60)) / 60;
            int longestStreamMinutes = longestStreamtime % 60;

            int ingameTimeDays = ingameTime / (24 * 60);
            int ingameTimeHours = (ingameTime % (24 * 60)) / 60;
            int ingameTimeMinutes = ingameTime % 60;

            // Change box text to the variables
            saveSlotBox.Text = $"{saveSlot + 1}"; // +1 to represent the in-game save slot number.
            infoSaveBox.Text = $"{saveSlot + 1}";
            botNameBox.Text = $"{botName}";
            moneyTextBox.Text = $"{moneyVal}$";
            casinoTokenBox.Text = $"{casinoTokensVal}";
            maxCumBox.Text = $"{maxCumVal}ml";
            currentCumBox.Text = $"{currentCumVal}ml";
            gameVersionBox.Text = $"{gameVersion}";
            currentStaminaBox.Text = $"{currentStamina * 100}"; // Do "* 100" to make it an "x out of 100" value instead of a "0.xxxx<...>" value for readability.
            playerNameBox.Text = $"{playerName}";
            playerHealthBox.Text = $"{health * 100}"; // Do "* 100" to make it an "x out of 100" value instead of a "0.xxxx<...>" value for readability.
            mentalHealthBox.Text = $"{mentalHealth * 100}"; // Do "* 100" to make it an "x out of 100" value instead of a "0.xxxx<...>" value for readability.
            botLustBox.Text = $"{botLust}";
            botSympathyBox.Text = $"{botSympathy}";
            botLongingBox.Text = $"{botLonging}";
            botMoodBox.Text = $"{botMood}";
            botIntBox.Text = $"{botInt}";
            cumInPussy.Text = $"{cumInPussyVal}ml";
            cumInAss.Text = $"{cumInAssVal}ml";
            cumInStomach.Text = $"{cumInStomachVal}ml";
            gameStageBox.Text = $"{gameStage}";
            subsTextBox.Text = $"{subs}";
            followersTextBox.Text = $"{followers}";
            totalStreamTimeRawBox.Text = $"{totalStreamtime}";
            totalStreamTimeFormattedBox.Text = $"{streamDays}d;{streamHours}h;{streamMinutes}m";
            streamDonations.Text = $"{streamDonationsVal}$";
            longestStreamRawBox.Text = $"{longestStreamtime}";
            longestStreamFormattedBox.Text = $"{longestStreamDays}d;{longestStreamHours}h;{longestStreamMinutes}m";
            timesCumInsideVag.Text = $"{timesCameInsideVal} times";
            timesCumInsideAss.Text = $"{timesCameInsideAnalVal} times";
            timesCumInsideOral.Text = $"{timesCameInMouthVal} times";
            cameOutsideBox.Text = $"{timesCameOutside} times";
            gameTimeRaw.Text = $"{ingameTime}";
            gameTimeFormatted.Text = $"{ingameTimeDays}d;{ingameTimeHours}h;{ingameTimeMinutes}m";

            // If Lightswitch is on, set checked, else set unchecked.
            if (lightSwitchOn)
            {
                lightswitchCheckbox.Checked = true;
            }
            else { 
                lightswitchCheckbox.Checked = false;
            }

            // Visited Websites checker
            List<string> itemsToCheck = new List<string>();

            foreach (var item in websitesCheckBoxes.Items)
            {
                if (visitedWebsites.Contains(item.ToString()))
                {
                    itemsToCheck.Add(item.ToString());
                }
            }

            foreach (var item in itemsToCheck)
            {
                websitesCheckBoxes.SetItemChecked(websitesCheckBoxes.Items.IndexOf(item), true);
            }

            // Gotten Achievements checker
            List<string> achievementsToCheck = new List<string>();
            foreach (Control groupBox in achievementsPanel.Controls)
            {
                if (groupBox is GroupBox)
                {
                    GroupBox currentGroupBox = (GroupBox)groupBox;

                    foreach (Control control in currentGroupBox.Controls)
                    {
                        if (control is CheckedListBox)
                        {
                            CheckedListBox currentCheckedListBox = (CheckedListBox)control;

                            foreach (var item in currentCheckedListBox.Items)
                            {
                                if (gottenAchievements.Contains(item.ToString()))
                                {
                                    achievementsToCheck.Add(item.ToString());
                                }
                            }

                            foreach (var item in achievementsToCheck)
                            {
                                int index = currentCheckedListBox.Items.IndexOf(item);
                                if (index != -1)
                                {
                                    currentCheckedListBox.SetItemChecked(currentCheckedListBox.Items.IndexOf(item), true);
                                }
                            }
                        }
                    }
                }
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

        private void ShowUpdatePopup(bool isNewest, System.Version current, System.Version latest) // Show on clicking "Check for Updates", pass "isNewest" on whether there is or isn't a new version
        {
            if (!isNewest) // If not newest, show this
            {
                DialogResult result = MessageBox.Show($"New version found!\nYour Version: {current}\nNewest Version: {latest}\nDo you want to open GitHub to get the latest release?", "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                switch (result)
                {
                    case DialogResult.Yes:
                        openWebsite(repoUrl + "/releases/latest");
                        break;
                    case DialogResult.No: // Just close
                        break;
                }
            }
            else // If newest, show this
            {
                // Tell the User no new version found
                MessageBox.Show("You are already on the newest version.", "No new update found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string owner = "Wehrmachtserdbeere";
            string repo = "MDRG-Analyzer";
            HttpClient thisProgram = new HttpClient();

            try
            {
                {
                    thisProgram.DefaultRequestHeaders.Add("User-Agent", "request"); // Required for GitHub API

                    HttpResponseMessage gitResponse = await thisProgram.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/latest"); // Get the API URL

                    if (gitResponse.IsSuccessStatusCode)
                    {
                        string gitResponseBody = await gitResponse.Content.ReadAsStringAsync();
                        dynamic gitResponseInfo = JsonConvert.DeserializeObject(gitResponseBody);
                        string gitLatestVersion = gitResponseInfo.tag_name;

                        Console.WriteLine(gitLatestVersion);

                        if (Version.TryParse(gitLatestVersion.TrimStart('v'), out Version latest) && Version.TryParse(__version__, out Version current))
                        {
                            Console.WriteLine("Arrived here!");
                            int comparisonResult = current.CompareTo(latest);
                            if (comparisonResult < 0)
                            {
                                ShowUpdatePopup(false, current, latest);
                            }
                            else
                            {
                                ShowUpdatePopup(true, current, latest);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid version format.");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Error: {gitResponse.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException exep) // Exception Handler, tell User that there was an error
            {
                DialogResult result = MessageBox.Show($"Error: {exep.Message}\nPlease try again.", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                switch (result)
                {
                    case DialogResult.Retry:
                        checkForUpdatesToolStripMenuItem_Click(sender, e);
                        break;
                    case DialogResult.Cancel:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!achievementsPanel.Visible)
            {
                achievementsPanel.Visible = true;
            }
            else if (achievementsPanel.Visible)
            {
                achievementsPanel.Visible = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!visitedWebsiteGroupBox.Visible)
            {
                visitedWebsiteGroupBox.Visible = true;
            }
            else if (visitedWebsiteGroupBox.Visible)
            {
                visitedWebsiteGroupBox.Visible = false;
            }
        }

        private void reportABugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openWebsite(repoUrl + "/issues");
        }

        private void suggestAFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openWebsite(repoUrl + "/discussions/categories/ideas");
        }

        private void quickLinkToGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openWebsite(repoUrl);
        }

        private void openReadmeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filePath = "README.md";

            // Combine the current directory with the file name
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);

            if (File.Exists(fullPath))
            {
                Process.Start(filePath);
            }
            else
            {
                Console.WriteLine("File not found: " + fullPath);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void infoSaveButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Because of game code and my lack of understanding, to select the correct Save Slot, select a File at the top, then check if the File has the correct Save Slot (i.E. You may have selected \"File 1\" but it actually loads \"Save Slot 4\". In the box next to this button, the correct slot will be listed.", "Save Slot Information");
        }
    }
}
