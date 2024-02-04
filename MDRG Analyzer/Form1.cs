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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Runtime.CompilerServices;

namespace MDRG_Analyzer
{
    public partial class Form1 : Form
    {
        // Initialize some variables
        string fileContent;
        JObject saveFileJson;
        string __version__ = "1.1.2";
        int selectedSaveFile = -1;
        string filePath;
        string repoUrl = "https://github.com/Wehrmachtserdbeere/MDRG-Analyzer";
        string botName = "your Bot";
        dynamic jsonData;

        public Form1()
        {
            InitializeComponent();
            versionBox.Text = $"{__version__}";
            CheckForUpdate(true);
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
                System.Windows.Forms.RadioButton radioButton = new System.Windows.Forms.RadioButton();
                radioButton.Text = "File " + i;
                radioButton.Name = "radioButton" + i.ToString();
                radioButton.AutoSize = true;

                radioButton.CheckedChanged += new EventHandler(RadioButton_CheckedChanged);

                flowLayoutPanel1.Controls.Add(radioButton);
            }
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.RadioButton radioButton = sender as System.Windows.Forms.RadioButton;
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
                filePath = openFileDialog.FileName;

                // Load save file into variable, then parse the JSon
                fileContent = File.ReadAllText(filePath);
                saveFileJson = JObject.Parse(fileContent);

                // Deserialize into dynamic object
                jsonData = JsonConvert.DeserializeObject(fileContent);

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
            string statusText = savedataObject.Value<string>("statusText");

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
            int streamCount = savedataObject.Value<int>("streamCount");
            int timesLostChess = savedataObject.Value<int>("timesLostChess");
            int timesWonChess = savedataObject.Value<int>("timesWonChess");
            int timesLostOldMaid = savedataObject.Value<int>("timesLostOldMaid");
            int timesWonOldMaid = savedataObject.Value<int>("timesWonOldMaid");
            int timesRanAwayOldMaid = savedataObject.Value<int>("timesRanAwayOldMaid");
            int timesLostWordChain = savedataObject.Value<int>("timesLostWordChain");
            int timesWonWordChain = savedataObject.Value<int>("timesWonWordChain");

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

            // 1.1.0 New stuff
            double mlCameInMouth = savedataObject.Value<double>("mlCameInMouth");
            double mlOfCumWasted = savedataObject.Value<double>("mlOfCumWasted");
            double search = savedataObject.Value<double>("search"); // In the savegame it's in the church events, so it might be part of it.

            // All depend on gametime I think
            int vinegaraEffectEnd = savedataObject.Value<int>("vinegaraEffectEnd");
            int deathGripEffectEnd = savedataObject.Value<int>("deathGripEffectEnd");
            int timesWentToChurch = savedataObject.Value<int>("timesWentToChurch");
            int lastMentalHealthInfoAt = savedataObject.Value<int>("lastMentalHealthInfoAt");
            int lastHungerInfoAt = savedataObject.Value<int>("lastHungerInfoAt");
            int lastHeadpatedAt = savedataObject.Value<int>("lastHeadpatedAt");
            int lastBotStartedTalkAt = savedataObject.Value<int>("lastBotStartedTalkAt");
            int lastStreamedAt = savedataObject.Value<int>("lastStreamedAt");
            int lastOutsideWithBotAt = savedataObject.Value<int>("lastOutsideWithBotAt");
            int lastEquipmentAt = savedataObject.Value<int>("lastEquipmentAt");
            int lastInteractAt = savedataObject.Value<int>("lastInteractAt");
            int lastFuckedAt = savedataObject.Value<int>("lastFuckedAt");
            int lastWokeUpAt = savedataObject.Value<int>("lastWokeUpAt");
            int lastWentToChurchAt = savedataObject.Value<int>("lastWentToChurchAt");
            int lastWorkedAtDay = savedataObject.Value<int>("lastWorkedAtDay");
            int nunPoints = savedataObject.Value<int>("nunPoints");
            int priestBotPoints = savedataObject.Value<int>("priestBotPoints");

            // 0.87.4 new stuff
            int lastCuddleAt = savedataObject.Value<int>("lastCuddledAt");

            // Change box text to the variables
            saveSlotBox.Text = $"{saveSlot + 1}"; // +1 to represent the in-game save slot number.
            saveSlotBoxBot.Text = $"{saveSlot + 1}";
            saveSlotBoxGen.Text = $"{saveSlot + 1}";
            infoSaveBox.Text = $"{saveSlot + 1}";
            botNameBox.Text = $"{botName}";
            moneyTextBox.Text = $"{moneyVal}";
            casinoTokenBox.Text = $"{casinoTokensVal}";
            maxCumBox.Text = $"{maxCumVal}";
            currentCumBox.Text = $"{currentCumVal}";
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
            cumInPussy.Text = $"{cumInPussyVal}";
            cumInAss.Text = $"{cumInAssVal}";
            cumInStomach.Text = $"{cumInStomachVal}";
            gameStageBox.Text = $"{gameStage}";
            subsTextBox.Text = $"{subs}";
            followersTextBox.Text = $"{followers}";
            totalStreamTimeRawBox.Text = $"{totalStreamtime}";
            totalStreamTimeFormattedBox.Text = $"{streamDays}d;{streamHours}h;{streamMinutes}m";
            streamDonations.Text = $"{streamDonationsVal}";
            longestStreamRawBox.Text = $"{longestStreamtime}";
            longestStreamFormattedBox.Text = $"{longestStreamDays}d;{longestStreamHours}h;{longestStreamMinutes}m";
            timesCumInsideVag.Text = $"{timesCameInsideVal}";
            timesCumInsideAss.Text = $"{timesCameInsideAnalVal}";
            timesCumInsideOral.Text = $"{timesCameInMouthVal}";
            cameOutsideBox.Text = $"{timesCameOutside}";
            gameTimeRaw.Text = $"{ingameTime}";
            gameTimeFormatted.Text = $"{ingameTimeDays}d;{ingameTimeHours}h;{ingameTimeMinutes}m";
            statusTextBox.Text = $"{statusText}";
            streamCountBox.Text = $"{streamCount}";
            timesWonOldMaidBox.Text = $"{timesWonOldMaid}";
            timesLostOldMaidBox.Text = $"{timesLostOldMaid}";
            timesRanAwayOldMaidBox.Text = $"{timesRanAwayOldMaid}";
            timesWonChessBox.Text = $"{timesWonChess}";
            timesLostChessBox.Text = $"{timesLostChess}";
            timesLostWordChainBox.Text = $"{timesLostWordChain}";
            timesWonWordChainBox.Text = $"{timesWonWordChain}";

            // 1.1.0 New Stuff

            mlCameInMouthBox.Text = $"{mlCameInMouth}";
            mlCumWastedBox.Text = $"{mlOfCumWasted}";
            searchTextBox.Text = $"{search}";
            vinegaraEndBox.Text = $"{vinegaraEffectEnd}";
            deathGripEffectEndBox.Text = $"{deathGripEffectEnd}";
            churchAmountBox.Text = $"{timesWentToChurch}";
            lastMentalHealthInfoAtBox.Text = $"{lastMentalHealthInfoAt}";
            lastHungerInfoAtBox.Text = $"{lastHungerInfoAt}";
            lastHeadpatedAtBox.Text = $"{lastHeadpatedAt}";
            lastBotStartedTalkAtBox.Text = $"{lastBotStartedTalkAt}";
            lastStreamedAtBox.Text = $"{lastStreamedAt}";
            lastOutsideWithBotAtBox.Text = $"{lastOutsideWithBotAt}";
            lastEquipmentAtBox.Text = $"{lastEquipmentAt}";
            lastInteractAtBox.Text = $"{lastInteractAt}";
            lastFuckedAtBox.Text = $"{lastFuckedAt}";
            lastWokeUpAtBox.Text = $"{lastWokeUpAt}";
            lastWentToChurchAtBox.Text = $"{lastWentToChurchAt}";
            lastWorkedAtDayBox.Text = $"{lastWorkedAtDay}";
            nunPointsBox.Text = $"{nunPoints}";
            priestBotPointsBox.Text = $"{priestBotPoints}";

            // 0.87.4 New stuff

            lastCuddledAtBox.Text = $"{lastCuddleAt}";


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
                if (groupBox is System.Windows.Forms.GroupBox)
                {
                    System.Windows.Forms.GroupBox currentGroupBox = (System.Windows.Forms.GroupBox)groupBox;

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

        private void ShowUpdatePopup(bool isNewest, System.Version current, System.Version latest, bool isStartup) // Show on clicking "Check for Updates", pass "isNewest" on whether there is or isn't a new version
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
                if (!isStartup)
                {
                    MessageBox.Show("You are already on the newest version.", "No new update found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckForUpdate(false);
        }
        private async void CheckForUpdate(bool isStartup)
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
                            int comparisonResult = current.CompareTo(latest);
                            if (comparisonResult < 0)
                            {
                                ShowUpdatePopup(false, current, latest, isStartup);
                            }
                            else
                            {
                                ShowUpdatePopup(true, current, latest, isStartup);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Could not check for Update:\nInvalid version format.");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Could not check for Update:\nError: {gitResponse.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException exep) // Exception Handler, tell User that there was an error
            {
                DialogResult result = MessageBox.Show($"Error: {exep.Message}\nPlease try again.", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                switch (result)
                {
                    case DialogResult.Retry:
                        CheckForUpdate(false);
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
            string readmeFilePath = "README.md";

            // Combine the current directory with the file name
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, readmeFilePath);

            if (File.Exists(fullPath))
            {
                Process.Start(readmeFilePath);
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

        private void editInfoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"It's currently beyond my ability to add a save editor. If you know how to do that and want to contribute, please do so. Quick links are in the \"Help\" Menu.", "Notice");
        }

        private void createBackupButton_Click(object sender, EventArgs e)
        {
            if (filePath == null)
            {
                MessageBox.Show(caption: "Error", text: "No file selected. Please select a save file before creating a backup.");
            }
            else
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                string backupFileNamePattern = "save_backup_*.mdrg";
                string[] existingBackupFiles = Directory.GetFiles(directoryPath, backupFileNamePattern);
                string newBackupFileName;
                if (existingBackupFiles.Length > 0)
                {
                    // Find the highest number used for backup
                    int highestNumber = 0;
                    foreach (string filePaths in existingBackupFiles)
                    {
                        string fileName = Path.GetFileName(filePaths);
                        if (fileName.StartsWith("save_backup_") && fileName.EndsWith(".mdrg"))
                        {
                            string numberString = fileName.Replace("save_backup_", "").Replace(".mdrg", "");
                            if (int.TryParse(numberString, out int number))
                            {
                                highestNumber = Math.Max(highestNumber, number);
                            }
                        }
                    }

                    // Increment the highest number to get the new backup number
                    highestNumber++;
                    newBackupFileName = Path.Combine(directoryPath, $"save_backup_{highestNumber}.mdrg");
                }
                else
                {
                    // If no existing backups, create the first one
                    newBackupFileName = Path.Combine(directoryPath, "save_backup_1.mdrg");
                }
                File.Copy(filePath, newBackupFileName);

                MessageBox.Show(caption: "Success!", text: $"Backup completed successfully! Saved as {newBackupFileName}");
            }
        }


        private void saveEditConsentBox_CheckedChanged(object sender, EventArgs e)
        {
            ControlExtensions.ToggleControlsEnabled(groupBox3); // General Info
            ControlExtensions.ToggleControlsEnabled(groupBox15); // Bot Info
            ControlExtensions.ToggleControlsEnabled(groupBox25); // Misc Info
            ControlExtensions.ToggleControlsEnabled(saveEditGroupBox); // Save Edit Box

        }

        private void guideButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                caption: "Guide and Instructions",
                text: "Create Backup: This is the first thing you should do. It will create a backup, not overwrite it (unless you have more than 2147483647 backups, which will error out... I think).\n\n" +
                    "Follow the entry numbering. For example: If the value is \"42,927\", then use a decimal value like \"83,145\".\n\n" +
                    "It is unknown if special characters break the Names, so to be safe, only use letters \"A-Z\" and \"a-z\". If you have the time to test it, please open an issue with your findings on what works and what doesn't.\n\n" +
                    "It is recommended to not change the Game Stage.\n\n" +
                    "If a value has an asterisk next to it, it has some sort of issue associated with it. The issue(s) will be listed at the bottom of the tab.\n\n" +
                    "Values are usually integers, so assume the max. value to be 2147483647 (2,147,483,647)"
                );
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (saveSlotBox.Text != null)
            {
                try
                {
                    JObject savedataObject = JObject.Parse(saveFileJson["saves"][selectedSaveFile]["savedata"].ToString());
                    JObject achievementsObject = JObject.Parse(saveFileJson["achievements"].ToString()); // Achievements

                    savedataObject["playerName"] = playerNameBox.Text;
                    savedataObject["botName"] = botNameBox.Text;
                    savedataObject["money"] = Int32.Parse(moneyTextBox.Text);
                    savedataObject["casinoTokens"] = Int32.Parse(casinoTokenBox.Text);
                    savedataObject["_lust"] = Int32.Parse(botLustBox.Text);
                    savedataObject["_sympathy"] = Int32.Parse(botSympathyBox.Text);
                    savedataObject["_longing"] = Int32.Parse(botLongingBox.Text);
                    savedataObject["inteligence"] = Int32.Parse(botIntBox.Text);
                    savedataObject["subs"] = Int32.Parse(subsTextBox.Text);
                    savedataObject["followers"] = Int32.Parse(followersTextBox.Text);
                    savedataObject["streamedFor"] = totalStreamTimeRawBox.Text; // Why is this erroring out?
                    savedataObject["moneyEarnedFromDonations"] = streamDonations.Text;
                    savedataObject["longestStream"] = longestStreamRawBox.Text;
                    savedataObject["timesCameInside"] = timesCumInsideVag.Text;
                    savedataObject["timesCameInsideAnal"] = timesCumInsideAss.Text;
                    savedataObject["timesCameInMouth"] = timesCumInsideOral.Text;
                    savedataObject["timesCameOutside"] = cameOutsideBox.Text;
                    savedataObject["ingameTime"] = gameTimeRaw.Text;
                    savedataObject["streamCount"] = streamCountBox.Text;
                    savedataObject["timesLostChess"] = timesLostChessBox.Text;
                    savedataObject["timesWonChess"] = timesWonChessBox.Text;
                    savedataObject["timesLostOldMaid"] = timesLostOldMaidBox.Text;
                    savedataObject["timesWonOldMaid"] = timesWonOldMaidBox.Text;
                    savedataObject["timesRanAwayOldMaid"] = timesRanAwayOldMaidBox.Text;
                    savedataObject["timesLostWordChain"] = timesLostWordChainBox.Text;
                    savedataObject["timesWonWordChain"] = timesWonWordChainBox.Text;

                    savedataObject["_maxCum"] = maxCumBox.Text;
                    savedataObject["_remainingCum"] = currentCumBox.Text;
                    savedataObject["_health"] = playerHealthBox.Text;
                    savedataObject["_stamina"] = currentStaminaBox.Text;
                    savedataObject["_mentalHealth"] = mentalHealthBox.Text;
                    savedataObject["_mood"] = botMoodBox.Text;
                    savedataObject["_cumInside"] = cumInPussy.Text;
                    savedataObject["_cumInsideAnal"] = cumInAss.Text;
                    savedataObject["_cumInsideStomach"] = cumInStomach.Text;

                    savedataObject["mlCameInMouth"] = mlCameInMouthBox.Text;
                    savedataObject["mlOfCumWasted"] = mlCumWastedBox.Text;
                    savedataObject["search"] = searchTextBox.Text;
                    savedataObject["vinegaraEffectEnd"] = vinegaraEndBox.Text;
                    savedataObject["deathGripEffectEnd"] = deathGripEffectEndBox.Text;
                    savedataObject["timesWentToChurch"] = churchAmountBox.Text;
                    savedataObject["lastMentalHealthInfoAt"] = lastMentalHealthInfoAtBox.Text;
                    savedataObject["lastHungerInfoAt"] = lastHungerInfoAtBox.Text;
                    savedataObject["lastHeadpatedAt"] = lastHeadpatedAtBox.Text;
                    savedataObject["lastBotStartedTalkAt"] = lastBotStartedTalkAtBox.Text;
                    savedataObject["lastStreamedAt"] = lastStreamedAtBox.Text;
                    savedataObject["lastOutsideWithBotAt"] = lastOutsideWithBotAtBox.Text;
                    savedataObject["lastEquipmentAt"] = lastEquipmentAtBox.Text;
                    savedataObject["lastInteractAt"] = lastInteractAtBox.Text;
                    savedataObject["lastFuckedAt"] = lastFuckedAtBox.Text;
                    savedataObject["lastWokeUpAt"] = lastWokeUpAtBox.Text;
                    savedataObject["lastWentToChurchAt"] = lastWentToChurchAtBox.Text;
                    savedataObject["lastWorkedAtDay"] = lastWorkedAtDayBox.Text;
                    savedataObject["nunPoints"] = nunPointsBox.Text;
                    savedataObject["priestBotPoints"] = priestBotPointsBox.Text;

                    // 0.87

                    savedataObject["lastCuddledAt"] = lastCuddledAtBox.Text;

                    if (lightswitchCheckbox.Checked)
                    {
                        savedataObject["lightSwitchOn"] = true;
                    }
                    else
                    {
                        savedataObject["lightSwitchOn"] = false;
                    }

                    try
                    {
                        string updatedSavedataJson = savedataObject.ToString();
                        saveFileJson["saves"][selectedSaveFile]["savedata"] = updatedSavedataJson;
                        string updatedAchievementsJson = achievementsObject.ToString();
                        saveFileJson["achievements"] = updatedAchievementsJson;
                        string finalJson = saveFileJson.ToString();
                        File.WriteAllText(filePath, finalJson);
                        MessageBox.Show(caption: "Success", text: "Successfully saved!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(caption: "Error", text: $"Exception: {ex}");
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(caption: "Error", text: $"Error: {ex}");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                caption: "Extra Credits",
                text: "Extra Credits go to:\n\n" +
                "The MDRG Discord - Coding help\n" +
                "italy2003 / PixivID: 66835722 - Art\n" +
                "bgrmystr2 - Linux Testing\n" +
                "You - For using this program. Thank you :D"
                );
        }

        private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}

// TODO:
// A quick menu that you can open or close on the side that can navigate to a folder full of saves. It would show up on the left with all the save files in the folder it's set to open (The user
//   can set that folder to w/e) and this means it'll be easy to move a save to that folder, open the editor, and just grab the save from the list. If they don't want the list, it can just collapse
//   into a bar on the side.
// 
// A way to compare two different saves, perhaps to see which one is better or to see if a save lacks a stat a different one has which could identify if the player missed an event or something

public static class ControlExtensions
{
    public static void ToggleControlsEnabled(this Control control)
    {
        if (control is System.Windows.Forms.GroupBox groupBox)
        {
            // Loop through each control inside the GroupBox
            foreach (Control childControl in groupBox.Controls)
            {
                // If the child control is another GroupBox, recursively call the method
                if (childControl is System.Windows.Forms.GroupBox subGroupBox)
                {
                    ToggleControlsEnabled(subGroupBox);
                }
                else
                {
                    // Toggle the ReadOnly property if the control is a RichTextBox or TextBox
                    if (childControl is RichTextBox richTextBox && childControl.Name != "statusTextBox")
                    {
                        richTextBox.ReadOnly = !richTextBox.ReadOnly;
                    }
                    else if (childControl is TextBox textBox && childControl.Name != "statusTextBox")
                    {
                        textBox.ReadOnly = !textBox.ReadOnly;
                    }
                    else if (childControl is Button button)
                    {
                        button.Enabled = !button.Enabled;
                    }
                    else if (childControl is System.Windows.Forms.CheckBox checkBox)
                    {
                        checkBox.Enabled = !checkBox.Enabled;
                    }
                }
            }
        }
    }
}

