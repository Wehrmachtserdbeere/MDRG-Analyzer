using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MDRG_Analyzer;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.IO;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using System.CodeDom;

namespace MDRG_Analyzer
{
    public partial class Form1 : Form
    {
        // Initialize some variables
        string fileContent;
        readonly string __version__ = "1.2.0";
        public int selectedSaveFile = -1;
        string filePath;
        readonly string repoUrl = "https://github.com/Wehrmachtserdbeere/MDRG-Analyzer";
        readonly string developerWebsite = "https://wehrmachtserdbeere.github.io/";
        readonly Random rand = new Random();
        public int eventInitiator = 0;
        readonly System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        Dictionary<(string, RichTextBox), Type> dataBindings = new Dictionary<(string, RichTextBox), Type> { };
        JsonNode savefileData;
        JsonNode savefileDataRoot;
        JsonNode savedataObject;
        JsonNode savefileRoot;
        JsonNode saveDataObjectJson;
        JsonNode achievementsObject;

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };

        public Form1()
        {
            // Initialize eventInitiator with a random between 0 and 99
            eventInitiator = rand.Next(minValue: 0, maxValue: 999);
            InitializeComponent(); // Initialize the form
            ChangeLanguage("en"); // Set the default language to English (actually to Default)
            CheckForUpdate(true); // Autocheck for updates
        }

        private void ResetLanguageMenuItems()
        {
            foreach (var item in languageToolStripMenuItem.DropDownItems) // For each item in the language menu
            {
                if (item is ToolStripMenuItem toolItem) // If the item is a ToolStripMenuItem
                {
                    toolItem.Checked = false; // Uncheck the item
                    toolItem.Font = new System.Drawing.Font( // Set the font of the item
                        toolItem.Font.FontFamily, // Default font family
                        toolItem.Font.Size, // Default font size
                        System.Drawing.FontStyle.Regular // Unbold the font
                    );
                }
            }
        }

        private void ChangeLanguage(string cultureCode) // Method to change the language
        {
            // Set the culture for the current thread
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode); // Set the UI culture
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureCode); // Set the general culture

            // Refresh UI elements
            this.Controls.Clear(); // Clear the controls
            InitializeComponent(); // Reinitialize form
            versionBox.Text = $"{__version__}"; // Ensure version is set

            // Reset language menu items
            ResetLanguageMenuItems();

            // Make a quick dictionary for all the languages and their associated ToolStripMenuItem
            var languageMappings = new Dictionary<string, ToolStripMenuItem>
            {
                { "de", deutschToolStripMenuItem },
                { "zh", traditionalChineseToolStripMenuItem },
                { "es", españolToolStripMenuItem },
                { "pt", portuguesaToolStripMenuItem },
                { "ja", japaneseToolStripMenuItem },
                { "en", englishToolStripMenuItem }
            };

            if (languageMappings.TryGetValue(cultureCode, out var menuItem)) // If the language is in the dictionary
            {
                SetLanguageAppearanceBoldChecked(menuItem); // Set the appearance of the language to be bold and checked
                // If the language is not English, German, or Chinese, show a machine translation notice
                if (cultureCode != "en" && cultureCode != "de" && cultureCode != "zh")
                {
                    MachineLanguageNotice();
                }
            }
            // If the language is invalid, default to English
            else
            {
                SetLanguageAppearanceBoldChecked(englishToolStripMenuItem);
            }
        }

        // Make specified menuItem bold and checked
        private void SetLanguageAppearanceBoldChecked(ToolStripMenuItem menuItem)
        {
            menuItem.Checked = true;
            menuItem.Font = new System.Drawing.Font(
                menuItem.Font.FontFamily,
                menuItem.Font.Size,
                System.Drawing.FontStyle.Bold
            );
        }

        // Notice for machine translated language in English
        private void MachineLanguageNotice()
        {
            MessageBox.Show(
                // Do NOT localize!
                "This language is machine translated and may not be accurate.",
                "Machine Translation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // Open website in the machine's default browser
        public static void OpenWebsite(string url)
        {
            Process.Start("cmd", $"/C start {url}");
        }

        // Load file on clicking loadfile
        public void loadToolStripMenuItem_Click(object sender, EventArgs e)
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

                // Load save file into variable
                fileContent = System.IO.File.ReadAllText(filePath);

                // Parse JSON into a JsonDocument
                savefileData = JsonNode.Parse(fileContent);
                savefileDataRoot = savefileData;

                // Extract saves array
                JsonArray? saves = savefileDataRoot["saves"] as JsonArray;
                List<int> slots = new List<int>();

                // For each save...
                if (saves != null)
                {
                    foreach (JsonNode? save in saves)
                    {
                        // Ensure the node is not null and contains the "slot" property
                        if (save?["slot"] is JsonValue slotValue && slotValue.TryGetValue(out int slotNumber))
                        {
                            // Add the slot number to the list
                            slots.Add(slotNumber);
                        }
                    }
                }

                AddRadioButtons(slots.Count());

                debugTextBox.Text = fileContent;
            }
        }

        // Add radio buttons for each save slot
        private void AddRadioButtons(int count)
        {
            // Clear the radio buttons
            flowLayoutPanel1.Controls.Clear();
            var radioButtons = Enumerable.Range(1, count)
                .Select(i =>
                {
                    return new RadioButton
                    {
                        Text = string.Format(Strings.RadioButtonFileText, i),
                        Name = $"radioButton {i.ToString()}",
                        Tag = i,
                        AutoSize = true
                    };
                }).ToArray();

            // For each radio button, add an event handler
            foreach (var radioButton in radioButtons)
            {
                radioButton.CheckedChanged += new EventHandler(RadioButton_CheckedChanged);
            }

            flowLayoutPanel1.Controls.AddRange(radioButtons);
        }

        // When a radio button is checked, update the selected save file
        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton.Checked)
            {
                if (radioButton.Tag is int saveNumber)
                {
                    selectedSaveFile = saveNumber - 1;
                }
            }
            ReloadValues();
        }

        private void ReloadValues()
        {
            try
            {
                try
                {
                    // Deserialize the saves array
                    JsonArray? savesArray = savefileDataRoot["saves"] as JsonArray;

                    if (savesArray != null)
                    {
                        foreach (JsonNode? save in savesArray)
                        {
                            if (save?["slot"] is JsonValue slotValue && slotValue.TryGetValue(out int slotNumber) && slotNumber == selectedSaveFile)
                            {
                                savefileRoot = save; // Store the matching save entry

                                if (save["savedata"] is JsonValue savedataValue && savedataValue.TryGetValue(out string newRawJson))
                                {
                                    savedataObject = JsonNode.Parse(newRawJson); // Deserialize the JSON string inside "savedata"
                                    Console.WriteLine(savedataObject);
                                }

                                break;
                            }
                        }
                    }

                    // Ensure savedataObject is treated as a proper JSON object, not a stringified JSON
                    if (savedataObject is JsonValue savedataText && savedataText.TryGetValue(out string newerRawJson))
                    {
                        if (!string.IsNullOrEmpty(newerRawJson) && newerRawJson.StartsWith("{"))
                        {
                            savedataObject = JsonNode.Parse(newerRawJson);
                        }
                    }

                    // Get the achievements object
                    achievementsObject = savefileDataRoot["achievements"];

                }
                catch (Exception)
                {
                    if (savedataObject.Equals(default))
                    {
                        MessageBox.Show(
                            caption: Strings.GenericErrorCaption,
                            text: Strings.SavedataLoadingNoSavedataErrorText,
                            buttons: MessageBoxButtons.OK,
                            icon: MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        MessageBox.Show(
                            caption: Strings.GenericErrorCaption,
                            text: Strings.SavedataLoadingErrorText,
                            buttons: MessageBoxButtons.OK,
                            icon: MessageBoxIcon.Error);
                        return;
                    }
                }

                // Set the variables as touple string
                dataBindings = new Dictionary<(string, RichTextBox), Type>
                {
                    { ("botName", botNameBox), typeof(string) },
                    { ("money", moneyTextBox), typeof(int) },
                    { ("gameVersion", gameVersionBox), typeof(string) },
                    { ("playerName", playerNameBox), typeof(string) },
                    { ("casinoTokens", casinoTokenBox), typeof(int) },
                    { ("_lust", botLustBox), typeof(int) },
                    { ("_sympathy", botSympathyBox), typeof(int) },
                    { ("_longing", botLongingBox), typeof(int) },
                    { ("inteligence", botIntBox), typeof(int) },
                    { ("stage", gameStageBox), typeof(int) },
                    { ("subs", subsTextBox), typeof(int) },
                    { ("followers", followersTextBox), typeof(int) },
                    { ("streamedFor", totalStreamTimeRawBox), typeof(int) },
                    { ("moneyEarnedFromDonations", streamDonations), typeof(int) },
                    { ("longestStream", longestStreamRawBox), typeof(int) },
                    { ("timesCameInside", timesCumInsideVag), typeof(int) },
                    { ("timesCameInsideAnal", timesCumInsideAss), typeof(int) },
                    { ("timesCameInMouth", timesCumInsideOral), typeof(int) },
                    { ("timesCameOutside", cameOutsideBox), typeof(int) },
                    { ("streamCount", streamCountBox), typeof(int) },
                    { ("timesLostChess", timesLostChessBox), typeof(int) },
                    { ("timesWonChess", timesWonChessBox), typeof(int) },
                    { ("timesLostOldMaid", timesLostOldMaidBox), typeof(int) },
                    { ("timesWonOldMaid", timesWonOldMaidBox), typeof(int) },
                    { ("timesRanAwayOldMaid", timesRanAwayOldMaidBox), typeof(int) },
                    { ("timesLostWordChain", timesLostWordChainBox), typeof(int) },
                    { ("timesWonWordChain", timesWonWordChainBox), typeof(int) },
                    { ("vinegaraEffectEnd", vinegaraEndBox), typeof(int) },
                    { ("deathGripEffectEnd", deathGripEffectEndBox), typeof(int) },
                    { ("timesWentToChurch", churchAmountBox), typeof(int) },
                    { ("lastMentalHealthInfoAt", lastMentalHealthInfoAtBox), typeof(int) },
                    { ("lastHungerInfoAt", lastHungerInfoAtBox), typeof(int) },
                    { ("lastHeadpatedAt", lastHeadpatedAtBox), typeof(int) },
                    { ("lastBotStartedTalkAt", lastBotStartedTalkAtBox), typeof(int) },
                    { ("lastStreamedAt", lastStreamedAtBox), typeof(int) },
                    { ("lastOutsideWithBotAt", lastOutsideWithBotAtBox), typeof(int) },
                    { ("lastEquipmentAt", lastEquipmentAtBox), typeof(int) },
                    { ("lastInteractAt", lastInteractAtBox), typeof(int) },
                    { ("lastFuckedAt", lastFuckedAtBox), typeof(int) },
                    { ("lastWokeUpAt", lastWokeUpAtBox), typeof(int) },
                    { ("lastWentToChurchAt", lastWentToChurchAtBox), typeof(int) },
                    { ("lastWorkedAtDay", lastWorkedAtDayBox), typeof(int) },
                    { ("nunPoints", nunPointsBox), typeof(int) },
                    { ("priestBotPoints", priestBotPointsBox), typeof(int) },
                    { ("lastCuddledAt", lastCuddledAtBox), typeof(int) },
                    { ("_maxCum", maxCumBox), typeof(int) },
                    { ("_remainingCum", currentCumBox), typeof(int) },
                    { ("_health", playerHealthBox), typeof(int) },
                    { ("_stamina", currentStaminaBox), typeof(int) },
                    { ("_mentalHealth", mentalHealthBox), typeof(int) },
                    { ("_mood", botMoodBox), typeof(int) },
                    { ("_cumInside", cumInPussy), typeof(int) },
                    { ("_cumInsideAnal", cumInAss), typeof(int) },
                    { ("_cumInsideStomach", cumInStomach), typeof(int) },
                    { ("mlCameInMouth", mlCameInMouthBox), typeof(int) },
                    { ("mlOfCumWasted", mlCumWastedBox), typeof(int) },
                    { ("search", searchTextBox), typeof(int) },
                };

                foreach (var pair in dataBindings)
                {
                    // Explicitly access key
                    var key = pair.Key;
                    // Explicitly access value
                    RichTextBox pairValue = key.Item2;
                    Type entryType = pair.Value;

                    if (savedataObject[key.Item1] is JsonNode element)
                    {
                        // Text is always String
                        pairValue.Text = element.ToString();

                        if (entryType == typeof(int))
                        {
                            pairValue.Tag = "int";
                        }
                        else if (entryType == typeof(string))
                        { 
                            pairValue.Tag = "string";
                        }
                        else if (entryType == typeof(double))
                        {
                            pairValue.Tag = "double";
                        }
                        else
                        {
                            // Don't apply a value
                            pairValue.Tag = string.Empty;
                        }
                    }
                }

                // Cover non-automatic fields
                rentTextBox.Text = savedataObject["statusText"].ToString();

                int saveSlot = Int32.Parse(savefileRoot["slot"].ToString());
                int ingameTime = Int32.Parse(savefileRoot["ingameTime"].ToString());

                JsonNode jsVisitedWebsites = savefileDataRoot["visitedWebsites"];
                JsonNode jsGottenAchievements = achievementsObject["values"];

                // Get the visited websites into a nice array
                string[] visitedWebsites = jsVisitedWebsites
                    .AsArray()  // Convert to JsonArray
                    .Select(element => element.ToString()) // Use ToString to get the string representation
                    .ToArray();

                // Get the gotten achievements into a nice array
                string[] gottenAchievements = jsGottenAchievements
                    .AsArray()  // Convert to JsonArray
                    .Select(element => element.ToString()) // Use ToString to get the string representation
                    .ToArray();


                // Continue with other variables
                bool lightSwitchOn = bool.Parse(savedataObject["lightSwitchOn"].ToString());

                // Convert raw time into days, hours, minutes
                // streamedFor
                int streamDays = Int32.Parse(savedataObject["streamedFor"].ToString()) / (24 * 60);
                int streamHours = Int32.Parse(savedataObject["streamedFor"].ToString()) / 60 % 24;
                int streamMinutes = Int32.Parse(savedataObject["streamedFor"].ToString()) % 60;

                // longestStream
                int longestStreamDays = Int32.Parse(savedataObject["longestStream"].ToString()) / (24 * 60);
                int longestStreamHours = Int32.Parse(savedataObject["longestStream"].ToString()) / 60 % 24;
                int longestStreamMinutes = Int32.Parse(savedataObject["longestStream"].ToString()) % 60;

                // ingameTime
                int ingameDays = ingameTime / (24 * 60);
                int ingameHours = ingameTime / 60 % 24;
                int ingameMinutes = ingameTime % 60;

                // Change box text to the variables
                string displaySaveSlot = (saveSlot + 1).ToString();
                saveSlotBox.Text = displaySaveSlot;
                saveSlotBoxBot.Text = displaySaveSlot;
                saveSlotBoxGen.Text = displaySaveSlot;
                infoSaveBox.Text = displaySaveSlot;

                totalStreamTimeFormattedBox.Text = $"{streamDays}d;{streamHours}h;{streamMinutes}m";
                longestStreamFormattedBox.Text = $"{longestStreamDays}d;{longestStreamHours}h;{longestStreamMinutes}m";

                gameTimeRaw.Text = ingameTime.ToString();
                gameTimeRaw.Tag = "int";
                gameTimeFormatted.Text = $"{ingameDays}d;{ingameHours}h;{ingameMinutes}m";

                // Rent days mapping (automated)
                var weekDays = new[]
                {
                    Strings.WeekDaysMonday, Strings.WeekDaysSunday, Strings.WeekDaysSaturday,
                    Strings.WeekDaysFriday, Strings.WeekDaysThursday, Strings.WeekDaysWednesday,
                    Strings.WeekDaysTuesday, Strings.WeekDaysMonday
                };

                var rentDaysMapping = Enumerable.Range(0, 8)
                    // Just ask ChatGPT, lamda is confusing :(
                    .ToDictionary(i => i == 0 ? "rent today!" : $"rent in {i} day{(i > 1 ? "s" : "")}",
                                  i => weekDays[i],
                                  StringComparer.OrdinalIgnoreCase);

                // Get weekday
                weekdayTextBox.Text = rentDaysMapping.TryGetValue(rentTextBox.Text, out string value) ? value : Strings.WeekDaysUnknown;

                // If lightswitch is on, set checked, else, set unchecked
                lightswitchCheckbox.Checked = lightSwitchOn;
                lightswitchCheckbox.Tag = "bool";

                // Visited Websites checker
                List<string> itemsToCheck = new List<string>();
                foreach (var item in websitesCheckBoxes.Items)
                {
                    if (visitedWebsites.Contains(item.ToString()))
                    {
                        itemsToCheck.Add(item.ToString());
                    }
                }
                // Check the visited websites
                foreach (var item in itemsToCheck)
                {
                    websitesCheckBoxes.SetItemChecked(websitesCheckBoxes.Items.IndexOf(item), true);
                }
                websitesCheckBoxes.Tag = "string";

                // Gotten achievements checker
                foreach (var checkedListBox in achievementsPanel.Controls.OfType<GroupBox>()
                    .SelectMany(groupBox => groupBox.Controls.OfType<CheckedListBox>()))
                {
                    for (int i = 0; i < checkedListBox.Items.Count; i++)
                    {
                        if (gottenAchievements.Contains(checkedListBox.Items[i].ToString()))
                        {
                            checkedListBox.SetItemChecked(i, true);
                        }
                    }
                }
                achievementsPanel.Tag = "string";

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

        private void ShowUpdatePopup(bool isNewest, Version current, Version latest, bool isStartup)
        {
            if (!isNewest)
            {
                if (MessageBox.Show(
                    caption: Strings.GitResponseNewVersionAvailableCaption,
                    text: $"{Strings.GitResponseNewVersionAvailableText1}{current}{Strings.GitResponseNewVersionAvailableText2}{latest}{Strings.GitResponseNewVersionAvailableText3}",
                    buttons: MessageBoxButtons.YesNo,
                    icon: MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    OpenWebsite($"{repoUrl}/releases/latest");
                }
            }
            else if (!isStartup)
            {
                MessageBox.Show(
                    caption: Strings.GitResponseNewestVersionCaption,
                    text: Strings.GitResponseNewestVersionText,
                    buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Information);
            }
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckForUpdate(false);
        }

        private async void CheckForUpdate(bool isStartup)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return;
            }
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
                        //var gitResponseInfo = JsonConvert.DeserializeObject<JObject>(gitResponseBody);
                        JsonDocument gitResponseInfo = JsonDocument.Parse(gitResponseBody);
                        string gitLatestVersion = gitResponseInfo.RootElement.GetProperty("tag_name").ToString();

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

        // Save Handling
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (!saveSlotBox.Text.Equals("null"))
            {
                try
                {
                    // Load "savefile" data into nestedData as JsonObject
                    JsonObject nestedData = JsonSerializer.Deserialize<JsonObject>(savedataObject.ToString(), options);

                    foreach (var pair in dataBindings)
                    {
                        var pairCollection = pair.Key;
                        string pairKey = pairCollection.Item1;
                        Control control = pairCollection.Item2;

                        if (control is TextBox textBox)
                        {
                            if (textBox.Tag.ToString() == "string")
                            {
                                nestedData[pairKey] = textBox.Text;
                            }
                            else if (textBox.Tag.ToString() == "int" && int.TryParse(textBox.Text, out int intValue))
                            {
                                nestedData[pairKey] = intValue;
                            }
                        }
                        else if (control is RichTextBox richTextBox)
                        {
                            if (richTextBox.Tag.ToString() == "string")
                            {
                                nestedData[pairKey] = richTextBox.Text;
                            }
                            else if (richTextBox.Tag.ToString() == "int" && int.TryParse(richTextBox.Text, out int intValue))
                            {
                                nestedData[pairKey] = intValue;
                            }
                        }
                        Console.WriteLine("Changed Value! --- " + nestedData[pairKey].ToString());
                    }

                    // Create list for selected achievements
                    List<string> achievementsSelected = new List<string>();

                    // Add the achievements
                    foreach (Object item in checkedListBox1.CheckedItems)
                    {
                        achievementsSelected.Add(item.ToString());
                    }
                    foreach (Object item in checkedListBox2.CheckedItems)
                    {
                        achievementsSelected.Add(item.ToString());
                    }

                    // Create a JsonArray for it and populate it
                    JsonArray achievementsSelectedJsonArray = new JsonArray();
                    foreach (var achievement in achievementsSelected)
                    {
                        achievementsSelectedJsonArray.Add(achievement);
                    }

                    nestedData["lightSwitchOn"] = (bool)lightswitchCheckbox.Checked;

                    // Add Websites
                    List<string> visitedWebsites = new List<string>();
                    foreach (var item in websitesCheckBoxes.CheckedItems)
                    {
                        visitedWebsites.Add(item.ToString());
                    }

                    // Create a new JsonArray for it and populate it too
                    JsonArray visitedWebsitesJsonArray = new JsonArray();
                    foreach (var website in visitedWebsites)
                    {
                        visitedWebsitesJsonArray.Add(website);
                    }

                    // Try to save
                    try
                    {
                        // Serialize for re-introduction
                        string updatedWebsiteData = JsonSerializer.Serialize(visitedWebsitesJsonArray, options);
                        string updatedAchievementData = JsonSerializer.Serialize(achievementsSelectedJsonArray, options);

                        // Deserialize the root to access the data more easily
                        JsonObject updatedRoot = JsonSerializer.Deserialize<JsonObject>(savefileDataRoot.ToString(), options);

                        // Extract saves array and update relevant data
                        var saves = updatedRoot["saves"].AsArray();  // Get the "saves" array from the JsonObject

                        foreach (JsonObject save in saves)
                        {
                            if (save["slot"].GetValue<int>() == selectedSaveFile)
                            {
                                // Correctly set the "savedata" value to the updated data
                                save["savedata"] = nestedData;  // Assign the serialized nested data directly to "savedata"
                                save["notes"] = "Save Edited!";
                            }
                        }

                        // Update websites and achievements data
                        updatedRoot["visitedWebsites"] = JsonNode.Parse(updatedWebsiteData); // Ensure it's properly parsed
                        updatedRoot["achievements"]["values"] = JsonNode.Parse(updatedAchievementData); // Also parsed here

                        // Serialize the updated root back
                        string updatedData = JsonSerializer.Serialize(updatedRoot, options);

                        // Save the updated JSON string to the file
                        File.WriteAllText(filePath, updatedData);

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
                catch (Exception ex)
                {
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
        private void ChangeLanguageMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                ChangeLanguage(menuItem.Tag.ToString());
            }
        }
    }
}

public static class ControlExtensions
{
    public static void ToggleControlsEnabled(this Control control)
    {
        if (control is GroupBox groupBox)
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
                    case CheckBox checkBox:
                        checkBox.Enabled = !checkBox.Enabled;
                        break;
                    case CheckedListBox checkListBox:
                        checkListBox.Enabled = !checkListBox.Enabled;
                        break;
                    case GroupBox subGroupBox:
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
    public async Task X1(Label a, PictureBox b, int c = 100) { byte[] d = Convert.FromBase64String(global::MDRG_Analyzer.Properties.Resources.skbdtlt); using (var ms = new MemoryStream(d)) { var e = System.Drawing.Image.FromStream(ms); b.Image = e; } b.Name = "pictureBox2"; b.TabStop = false; a.Text = Strings.label6HorrorText; await Task.Delay(c); b.Image = global::MDRG_Analyzer.Properties.Resources.mdrg_girl; b.Name = "pictureBox2"; b.TabStop = false; a.Text = Strings.label6NormalText; }

}