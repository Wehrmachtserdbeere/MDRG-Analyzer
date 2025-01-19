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
        readonly string __version__ = "1.1.13";
        int selectedSaveFile = -1;
        string filePath;
        readonly string repoUrl = "https://github.com/Wehrmachtserdbeere/MDRG-Analyzer";
        readonly string developerWebsite = "https://wehrmachtserdbeere.github.io/";
        dynamic jsonData;
        readonly Random rand = new Random();
        public int eventInitiator = 0;
        readonly System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        Dictionary<string, RichTextBox> dataBindings = new Dictionary<string, RichTextBox>{};

        public Form1()
        {
            eventInitiator = rand.Next( minValue: 0, maxValue: 999 );
            InitializeComponent();
            ChangeLanguage("en");
            CheckForUpdate(true);
        }

        private void ResetLanguageMenuItems()
        {
            foreach (var item in languageToolStripMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem toolItem)
                {
                    toolItem.Checked = false;
                    toolItem.Font = new System.Drawing.Font(
                        toolItem.Font.FontFamily,
                        toolItem.Font.Size,
                        System.Drawing.FontStyle.Regular
                    );
                }
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

            var languageMappings = new Dictionary<string, ToolStripMenuItem>
            {
                { "de", deutschToolStripMenuItem },
                { "zh", traditionalChineseToolStripMenuItem },
                { "es", españolToolStripMenuItem },
                { "pt", portuguesaToolStripMenuItem },
                { "ja", japaneseToolStripMenuItem },
                { "en", englishToolStripMenuItem }
            };

            if (languageMappings.TryGetValue(cultureCode, out var menuItem))
            {
                SetLanguageAppearanceBoldCHecked(menuItem);
                if (cultureCode != "en" && cultureCode != "de" && cultureCode != "zh")
                {
                    MachineLanguageNotice();
                }
            }
            else
            {
                SetLanguageAppearanceBoldCHecked(englishToolStripMenuItem);
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

        public static void MachineLanguageNotice()
        {
            MessageBox.Show(
                caption: Strings.MachineLanguageNoticeCaption,
                text: Strings.MachineLanguageNoticeText,
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Information
                );
        }

        public static void OpenWebsite(string x)
        {
            System.Diagnostics.Process.Start("cmd", "/C start" + " " + x);
        }

        private void AddRadioButtons(int numberOfFiles)
        {
            flowLayoutPanel1.Controls.Clear(); // Clear existing controls
            var radioButtons = Enumerable.Range(1, numberOfFiles)
            .Select(i => new RadioButton
            {
                Text = string.Format(Strings.RadioButtonFileText, i),
                Name = "radioButton" + i.ToString(),
                Tag = i,
                AutoSize = true
            })
            .ToArray();

            foreach (var radioButton in radioButtons)
            {
                radioButton.CheckedChanged += new EventHandler(RadioButton_CheckedChanged);
            }

            flowLayoutPanel1.Controls.AddRange(radioButtons);
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
            ReloadValues();
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
                int slotsAmount = saves.Count;
                AddRadioButtons(slotsAmount);

                debugTextBox.Text = fileContent; // Pump entire JSon into Debug

            }
        }
        private void ReloadValues()
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

                dataBindings = new Dictionary<string, RichTextBox>
                {
                    { "botName", botNameBox },
                    { "money", moneyTextBox },
                    { "gameVersion", gameVersionBox },
                    { "playerName", playerNameBox },
                    { "casinoTokens", casinoTokenBox },
                    { "_lust", botLustBox },
                    { "_sympathy", botSympathyBox },
                    { "_longing", botLongingBox },
                    { "inteligence", botIntBox },
                    { "stage", gameStageBox },
                    { "subs", subsTextBox },
                    { "followers", followersTextBox },
                    { "streamedFor", totalStreamTimeRawBox },
                    { "moneyEarnedFromDonations", streamDonations },
                    { "longestStream", longestStreamRawBox },
                    { "timesCameInside", timesCumInsideVag },
                    { "timesCameInsideAnal", timesCumInsideAss },
                    { "timesCameInMouth", timesCumInsideOral },
                    { "timesCameOutside", cameOutsideBox },
                    { "streamCount", streamCountBox },
                    { "timesLostChess", timesLostChessBox },
                    { "timesWonChess", timesWonChessBox },
                    { "timesLostOldMaid", timesLostOldMaidBox },
                    { "timesWonOldMaid", timesWonOldMaidBox },
                    { "timesRanAwayOldMaid", timesRanAwayOldMaidBox },
                    { "timesLostWordChain", timesLostWordChainBox },
                    { "timesWonWordChain", timesWonWordChainBox },
                    { "vinegaraEffectEnd", vinegaraEndBox },
                    { "deathGripEffectEnd", deathGripEffectEndBox },
                    { "timesWentToChurch", churchAmountBox },
                    { "lastMentalHealthInfoAt", lastMentalHealthInfoAtBox },
                    { "lastHungerInfoAt", lastHungerInfoAtBox },
                    { "lastHeadpatedAt", lastHeadpatedAtBox },
                    { "lastBotStartedTalkAt", lastBotStartedTalkAtBox },
                    { "lastStreamedAt", lastStreamedAtBox },
                    { "lastOutsideWithBotAt", lastOutsideWithBotAtBox },
                    { "lastEquipmentAt", lastEquipmentAtBox },
                    { "lastInteractAt", lastInteractAtBox },
                    { "lastFuckedAt", lastFuckedAtBox },
                    { "lastWokeUpAt", lastWokeUpAtBox },
                    { "lastWentToChurchAt", lastWentToChurchAtBox },
                    { "lastWorkedAtDay", lastWorkedAtDayBox },
                    { "nunPoints", nunPointsBox },
                    { "priestBotPoints", priestBotPointsBox },
                    { "lastCuddledAt", lastCuddledAtBox },
                    { "_maxCum", maxCumBox },
                    { "_remainingCum", currentCumBox },
                    { "_health", playerHealthBox },
                    { "_stamina", currentStaminaBox },
                    { "_mentalHealth", mentalHealthBox },
                    { "_mood", botMoodBox },
                    { "_cumInside", cumInPussy },
                    { "_cumInsideAnal", cumInAss },
                    { "_cumInsideStomach", cumInStomach },
                    { "mlCameInMouth", mlCameInMouthBox },
                    { "mlOfCumWasted", mlCumWastedBox },
                    { "search", searchTextBox },
                };

                foreach (var pair in dataBindings)
                {
                    string key = pair.Key; // Explicitly access the key
                    RichTextBox value = pair.Value; // Explicitly access the value
                    if (savedataObject[key] != null)
                    {
                        value.Text = savedataObject.Value<string>(key).ToString();
                    }
                }

                rentTextBox.Text = savedataObject["statusText"].ToString();

                int saveSlot = saveFileJson["saves"][selectedSaveFile].Value<int>("slot");
                int ingameTime = saveFileJson["saves"][selectedSaveFile].Value<int>("ingameTime");

                JArray jsVisitedWebsites = (JArray)saveFileJson["visitedWebsites"];
                JArray jsGottenAchievements = (JArray)achievementsObject["values"];
                string[] visitedWebsites = jsVisitedWebsites.ToObject<string[]>();
                string[] gottenAchievements = jsGottenAchievements.ToObject<string[]>();

                // Continue various variables
                bool lightSwitchOn = savedataObject.Value<bool>("lightSwitchOn");

                // Convert raw time into days, hours, and minutes
                int streamDays = savedataObject.Value<int>("streamedFor") / (24 * 60);
                int streamHours = (savedataObject.Value<int>("streamedFor") % (24 * 60)) / 60;
                int streamMinutes = savedataObject.Value<int>("streamedFor") % 60;
                
                int longestStreamDays = savedataObject.Value<int>("streamedFor") / (24 * 60);
                int longestStreamHours = (savedataObject.Value<int>("streamedFor") % (24 * 60)) / 60;
                int longestStreamMinutes = savedataObject.Value<int>("streamedFor") % 60;
                
                int ingameTimeDays = ingameTime / (24 * 60);
                int ingameTimeHours = (ingameTime % (24 * 60)) / 60;
                int ingameTimeMinutes = ingameTime % 60;


                // Change box text to the variables
                saveSlotBox.Text = $"{saveSlot + 1}"; // +1 to represent the in-game save slot number.
                saveSlotBoxBot.Text = $"{saveSlot + 1}";
                saveSlotBoxGen.Text = $"{saveSlot + 1}";
                infoSaveBox.Text = $"{saveSlot + 1}";
                totalStreamTimeFormattedBox.Text = $"{streamDays}d;{streamHours}h;{streamMinutes}m";
                longestStreamFormattedBox.Text = $"{longestStreamDays}d;{longestStreamHours}h;{longestStreamMinutes}m";
                gameTimeRaw.Text = $"{ingameTime}";
                gameTimeFormatted.Text = $"{ingameTimeDays}d;{ingameTimeHours}h;{ingameTimeMinutes}m";

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

                string weekDay = rentDaysMapping.TryGetValue(savedataObject["statusText"].ToString(), out var day) ? day : Strings.WeekDaysUnknown;

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
            catch (System.NullReferenceException)
            {
                MessageBox.Show(
                    caption: Strings.GenericErrorCaption,
                    text: Strings.SavedataLoadingNullErrorText,
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
                        OpenWebsite(repoUrl + "/releases/latest");
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
            OpenWebsite(repoUrl + "/issues");
        }

        private void suggestAFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebsite(repoUrl + "/discussions/categories/ideas");
        }

        private void quickLinkToGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebsite(repoUrl);
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

                    foreach (var pair in dataBindings)
                    {
                        string key = pair.Key;
                        Control control = pair.Value;

                        if (control is TextBox textBox)
                        {
                            savedataObject[key] = textBox.Text;
                        }
                        else if (control is RichTextBox richTextBox)
                        {
                            savedataObject[key] = richTextBox.Text;
                        }
                    }

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
            OpenWebsite("https://ko-fi.com/strawberrysoftware");
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
        private void japaneseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeLanguage("ja");
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
            OpenWebsite(developerWebsite);
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