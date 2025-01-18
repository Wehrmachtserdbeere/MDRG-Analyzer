using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MDRG_Analyzer;
using System.Drawing.Configuration;
using System.Drawing;

namespace MDRG_Analyzer
{
    public partial class Form1 : Form
    {
        // Initialize some variables
        string fileContent;
        JObject saveFileJson;
        readonly string __version__ = "1.1.12";
        int selectedSaveFile = -1;
        string filePath;
        readonly string repoUrl = "https://github.com/Wehrmachtserdbeere/MDRG-Analyzer";
        readonly string developerWebsite = "https://wehrmachtserdbeere.github.io/";
        dynamic jsonData;
        readonly Random rand = new Random();
        public int eventInitiator = 0;
        readonly System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));

        public Form1()
        {
            eventInitiator = rand.Next( minValue: 0, maxValue: 999 );
            InitializeComponent();
            ChangeLanguage("en");
            CheckForUpdate(true);
        }

        private void ResetLanguageMenuItems()
        {
            foreach (ToolStripMenuItem toolItem in languageToolStripMenuItem.DropDownItems)
            {
                toolItem.Checked = false;
                toolItem.Font = new System.Drawing.Font(
                    toolItem.Font.FontFamily,
                    toolItem.Font.Size,
                    System.Drawing.FontStyle.Regular
                );
            }
        }

        private void ChangeLanguage(string cultureCode)
        {
            // Set the culture for the current thread
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureCode);

            // Refresh UI elements (you may need to manually reload your form or update controls)
            this.Controls.Clear();
            InitializeComponent(); // Reinitialize form
            versionBox.Text = $"{__version__}";

            ResetLanguageMenuItems();

            switch (cultureCode)
            {
                case "de":
                    SetLanguageAppearanceBoldCHecked(deutschToolStripMenuItem);
                    break;
                case "zh":
                    SetLanguageAppearanceBoldCHecked(traditionalChineseToolStripMenuItem);
                    break;
                case "es":
                    SetLanguageAppearanceBoldCHecked(españolToolStripMenuItem);
                    break;
                case "pt":
                    SetLanguageAppearanceBoldCHecked(portuguesaToolStripMenuItem);
                    break;
                default:
                    SetLanguageAppearanceBoldCHecked(englishToolStripMenuItem);
                    break;
            }
        }

        private void SetLanguageAppearanceBoldCHecked(ToolStripMenuItem menuIten)
        {
            menuIten.Checked = true;
            menuIten.Font = new System.Drawing.Font(
                menuIten.Font.FontFamily,
                menuIten.Font.Size,
                System.Drawing.FontStyle.Bold
                );
        }

        public static void openWebsite(string x)
        {
            System.Diagnostics.Process.Start("cmd", "/C start" + " " + x);
        }

        private void AddRadioButtons(int numberOfFiles)
        {
            flowLayoutPanel1.Controls.Clear(); // Clear existing controls
            var radioButtons = new List<RadioButton>();

            for (int i = 1; i <= numberOfFiles; i++)
            {
                var radioButton = new RadioButton
                {
                    Text = string.Format(Strings.RadioButtonFileText, i), // Localized text
                    Name = "radioButton" + i.ToString(),
                    Tag = i, // Store the number in the Tag property
                    AutoSize = true
                };

                radioButton.CheckedChanged += new EventHandler(RadioButton_CheckedChanged);
                radioButtons.Add(radioButton);
            }

            flowLayoutPanel1.Controls.AddRange(radioButtons.ToArray());
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.RadioButton radioButton = sender as System.Windows.Forms.RadioButton;
            if (radioButton.Checked)
            {
                if (radioButton.Tag is int saveNumber)
                {
                    selectedSaveFile = saveNumber - 1;
                }
            }
            reloadValues();
        }


        public void loadToolStripMenuItem_Click(object sender, EventArgs e) // On clicking "Load File"
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = Strings.openFileDialogFilter,
                Title = Strings.openFileDialogTitle
            };

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
                int slotsAmount = saves.Count;
                AddRadioButtons(slotsAmount);

                debugTextBox.Text = fileContent; // Pump entire JSon into Debug

            }
        }
        private void reloadValues()
        {
            try
            {
                // Test that there is data
                JObject savedataObject = null;
                JObject achievementsObject = null;
                try
                {
                    savedataObject = JObject.Parse(saveFileJson["saves"][selectedSaveFile]["savedata"].ToString());
                    achievementsObject = JObject.Parse(saveFileJson["achievements"].ToString()); // Achievements
                }
                catch
                {
                    if (savedataObject == null)
                    {
                        MessageBox.Show(
                            caption: Strings.GenericErrorCaption,
                            text: Strings.SavedataLoadingNoSavedataErrorText,
                            buttons: MessageBoxButtons.OK,
                            icon: MessageBoxIcon.Error);
                        return;
                    }
                    else {
                        MessageBox.Show(
                            caption: Strings.GenericErrorCaption,
                            text: Strings.SavedataLoadingErrorText,
                            buttons: MessageBoxButtons.OK,
                            icon: MessageBoxIcon.Error);
                        return;
                    }
                }

                // Set the variables


                string botName = savedataObject.Value<string>("botName");
                string moneyVal = savedataObject.Value<string>("money");
                string gameVersion = savedataObject.Value<string>("gameVersion");
                string playerName = savedataObject.Value<string>("playerName");
                string rentText = savedataObject.Value<string>("statusText");

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
                rentTextBox.Text = $"{rentText}";
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

                // 0.90 New stuff

                var rentDaysMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "rent today!", Strings.WeekDaysMonday },
                    { "rent in 1 day", Strings.WeekDaysSunday },
                    { "rent in 2 days", Strings.WeekDaysSaturday },
                    { "rent in 3 days", Strings.WeekDaysFriday },
                    { "rent in 4 days", Strings.WeekDaysThursday },
                    { "rent in 5 days", Strings.WeekDaysWednesday },
                    { "rent in 6 days", Strings.WeekDaysTuesday },
                    { "rent in 7 days", Strings.WeekDaysMonday }
                };

                string weekDay = rentDaysMapping.TryGetValue(rentText, out var day) ? day : Strings.WeekDaysUnknown;

                weekdayTextBox.Text = $"{weekDay}";


                // If Lightswitch is on, set checked, else set unchecked.
                if (lightSwitchOn)
                {
                    lightswitchCheckbox.Checked = true;
                }
                else
                {
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
                foreach (Control groupBox in achievementsPanel.Controls)
                {
                    if (groupBox is System.Windows.Forms.GroupBox currentGroupBox)
                    {
                        foreach (Control control in currentGroupBox.Controls)
                        {
                            if (control is CheckedListBox currentCheckedListBox)
                            {
                                for (int i = 0; i < currentCheckedListBox.Items.Count; i++)
                                {
                                    var item = currentCheckedListBox.Items[i].ToString();
                                    if (gottenAchievements.Contains(item))
                                    {
                                        currentCheckedListBox.SetItemChecked(i, true);
                                    }
                                }
                            }
                        }
                    }
                }

            }

            catch (System.OverflowException)
            {
                MessageBox.Show(
                    caption: Strings.GenericErrorCaption,
                    text: Strings.SavedataLoadingOverflowErrorText,
                    buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Error
                    );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    caption: Strings.GenericErrorCaption,
                    text: Strings.SavedataLoadingUnknownError + ex.ToString(),
                    buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Error
                    );
            }
        }

        private void ShowUpdatePopup(bool isNewest, System.Version current, System.Version latest, bool isStartup) // Show on clicking "Check for Updates", pass "isNewest" on whether there is or isn't a new version
        {
            if (!isNewest) // If not newest, show this
            {
                DialogResult result = MessageBox.Show(
                    caption: Strings.GitResponseNewVersionAvailableCaption,
                    text: Strings.GitResponseNewVersionAvailableText1 + current.ToString() + Strings.GitResponseNewVersionAvailableText2 + latest.ToString() + Strings.GitResponseNewVersionAvailableText3,
                    buttons: MessageBoxButtons.YesNo,
                    icon: MessageBoxIcon.Information);

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
                    MessageBox.Show(
                        caption: Strings.GitResponseNewestVersionCaption,
                        text: Strings.GitResponseNewestVersionText,
                        buttons: MessageBoxButtons.OK,
                        icon: MessageBoxIcon.Information);
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

            using (HttpClient thisProgram = new HttpClient())
            {
                try
                {
                    thisProgram.DefaultRequestHeaders.Add("User-Agent", "request"); // Required for GitHub API

                    HttpResponseMessage gitResponse = await thisProgram.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/latest"); // Get the API URL

                    if (gitResponse.IsSuccessStatusCode)
                    {
                        string gitResponseBody = await gitResponse.Content.ReadAsStringAsync();
                        var gitResponseInfo = JsonConvert.DeserializeObject<JObject>(gitResponseBody);
                        string gitLatestVersion = gitResponseInfo["tag_name"].ToString();

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
                            MessageBox.Show(
                                caption: Strings.GenericErrorCaption,
                                text: Strings.GitResponseInvalidVersionFormatText
                            );
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            caption: Strings.GenericErrorCaption,
                            text: Strings.GitResponseCouldNotCheckForUpdateText + gitResponse.StatusCode.ToString()
                        );
                    }
                }
                catch (HttpRequestException exep) // Exception Handler, tell User that there was an error
                {
                    DialogResult result = MessageBox.Show(
                        caption: Strings.GenericErrorCaption,
                        text: Strings.HttpRequestExceptionMessageText + exep.Message,
                        buttons: MessageBoxButtons.RetryCancel,
                        icon: MessageBoxIcon.Error
                    );

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

                    DialogResult result = MessageBox.Show(
                        caption: Strings.GenericErrorCaption,
                        text: Strings.HttpRequestUnknownErrorMessageText + ex.Message,
                        buttons: MessageBoxButtons.RetryCancel,
                        icon: MessageBoxIcon.Error
                    );

                    switch (result)
                    {
                        case DialogResult.Retry:
                            CheckForUpdate(false);
                            break;
                        case DialogResult.Cancel:
                            break;
                    }
                }
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
            MessageBox.Show(
                caption: Strings.EditInfoButtonCaption,
                text: Strings.EditInfoButtonText
                );
        }

        private void createBackupButton_Click(object sender, EventArgs e)
        {
            if (filePath == null)
            {
                MessageBox.Show(
                    caption: Strings.GenericErrorCaption,
                    text: Strings.BackupNoFileSelectedText
                    );
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

                MessageBox.Show(
                    caption: Strings.GenericSuccessCaption,
                    text: Strings.BackupSuccessText + newBackupFileName.ToString()
                    );
            }
        }


        private void saveEditConsentBox_CheckedChanged(object sender, EventArgs e)
        {
            ControlExtensions.ToggleControlsEnabled(groupBox3); // General Info
            ControlExtensions.ToggleControlsEnabled(groupBox15); // Bot Info
            ControlExtensions.ToggleControlsEnabled(groupBox25); // Misc Info
            ControlExtensions.ToggleControlsEnabled(saveEditGroupBox); // Save Edit Box
            ControlExtensions.ToggleControlsEnabled(groupBox4); // Church Achievement Box
            ControlExtensions.ToggleControlsEnabled(groupBox5); // Misc Achievement Box
            websitesCheckBoxes.Enabled = !websitesCheckBoxes.Enabled; // Websites Visited Box

        }

        private void guideButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                caption: Strings.GuideAndInstructionsCaption,
                text: Strings.GuideAndInstructionsText
                );
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (saveSlotBox.Text != null)
            {
                try
                {
                    JObject savedataObject = JObject.Parse(saveFileJson["saves"][selectedSaveFile]["savedata"].ToString());
                    JObject achievementsObject = JObject.Parse(saveFileJson["achievements"].ToString());

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
                    savedataObject["streamedFor"] = totalStreamTimeRawBox.Text;
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

                    // Achievements (Temporarily disabled since I cannot figure out how to fix this right now.)

                    List<string> achievementsSelected = new List<string>();

                    foreach (Object item in checkedListBox1.CheckedItems)
                    {
                        achievementsSelected.Add($"{item}");
                    }
                    foreach (Object item in checkedListBox2.CheckedItems)
                    {
                        achievementsSelected.Add($"{item}");
                    }

                    JArray achievementsSelectedJArray = new JArray(achievementsSelected);
                    
                    foreach (string achievement in achievementsSelected)
                    {
                        achievementsSelectedJArray.Add(achievement);
                    }

                    if (lightswitchCheckbox.Checked)
                    {
                        savedataObject["lightSwitchOn"] = true;
                    }
                    else
                    {
                        savedataObject["lightSwitchOn"] = false;
                    }

                    // Add Websites

                    List<string> visitedWebsites = new List<string>();

                    foreach (var item in websitesCheckBoxes.CheckedItems)
                    {
                        visitedWebsites.Add(item.ToString());
                    }

                    JArray visitedWebsitesJArray = new JArray(visitedWebsites);

                    try
                    {
                        string updatedSavedataJson = savedataObject.ToString();
                        saveFileJson["saves"][selectedSaveFile]["savedata"] = updatedSavedataJson;
                        saveFileJson["achievements"]["values"] = achievementsSelectedJArray;
                        saveFileJson["visitedWebsites"] = visitedWebsitesJArray;
                        string finalJson = saveFileJson.ToString();
                        File.WriteAllText(filePath, finalJson);
                        MessageBox.Show(
                            caption: Strings.GenericSuccessCaption,
                            text: Strings.SavedataSavingSuccessText
                            );
                    }
                    catch
                    {
                        MessageBox.Show(
                            caption: Strings.GenericErrorCaption,
                            text: Strings.SavedataSavingErrorText
                            );
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(
                        caption: Strings.GenericErrorCaption,
                        text: Strings.SavedataLoadingUnknownError + ex
                        );
                }
            }
        }

        private void extraCreditsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                caption: Strings.ExtraCreditsCaption,
                text: Strings.ExtraCreditsText
                );
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openWebsite("https://ko-fi.com/strawberrysoftware");
        }

        private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLanguage("en-US");
        }

        private void deutschToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLanguage("de");
        }

        private void traditionalChineseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLanguage("zh");
        }

        private void españolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLanguage("es");
        }

        private void portuguesaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLanguage("pt");
        }

        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 6)
            {
                if (eventInitiator == 444) // Remove this later, this is a test
                {
                    AsynchonousTasks asynchonousTasks = new AsynchonousTasks();
                    await asynchonousTasks.X1(label6, pictureBox2);
                    eventInitiator = 0;
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            openWebsite(developerWebsite);
        }
    }
}

public static class ControlExtensions
{
    public static void ToggleControlsEnabled(this Control control)
    {
        if (control is System.Windows.Forms.GroupBox groupBox)
        {
            foreach (Control childControl in groupBox.Controls)
            {
                switch (childControl)
                {
                    case RichTextBox richTextBox when childControl.Name != "weekdayTextBox" && childControl.Name != "rentTextBox":
                        richTextBox.ReadOnly = !richTextBox.ReadOnly;
                        break;
                    case TextBox textBox when childControl.Name != "weekdayTextBox" && childControl.Name != "rentTextBox":
                        textBox.ReadOnly = !textBox.ReadOnly;
                        break;
                    case Button button:
                        button.Enabled = !button.Enabled;
                        break;
                    case System.Windows.Forms.CheckBox checkBox:
                        checkBox.Enabled = !checkBox.Enabled;
                        break;
                    case CheckedListBox checkListBox:
                        checkListBox.Enabled = !checkListBox.Enabled;
                        break;
                    case System.Windows.Forms.GroupBox subGroupBox:
                        ToggleControlsEnabled(subGroupBox);
                        break;
                }
            }
        }
    }
}

class AsynchonousTasks
{
    // This is a test method to see if async works. Remove this later.
    public async Task X1(System.Windows.Forms.Label a, PictureBox b, int c = 75) { byte[] d = Convert.FromBase64String(global::MDRG_Analyzer.Properties.Resources.skbdtlt); using (var ms = new MemoryStream(d)) { var e = System.Drawing.Image.FromStream(ms); b.Image = e; } b.Name = "pictureBox2"; b.TabStop = false; a.Text = Strings.label6HorrorText; await Task.Delay(c); b.Image = global::MDRG_Analyzer.Properties.Resources.mdrg_girl; b.Name = "pictureBox2"; b.TabStop = false; a.Text = Strings.label6NormalText; }

}