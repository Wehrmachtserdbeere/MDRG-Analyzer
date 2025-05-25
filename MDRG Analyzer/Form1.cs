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
using System.Drawing;

namespace MDRG_Analyzer
{
    public partial class Form1 : Form
    {
        // Initialize some variables
        string fileContent;
        JObject saveFileJson;
        readonly string __version__ = "1.1.19";
        int selectedSaveFile = -1;
        string filePath;
        readonly string repoUrl = "https://github.com/Wehrmachtserdbeere/MDRG-Analyzer";
        readonly string developerWebsite = "https://wehrmachtserdbeere.github.io/";
        readonly string MDRG_Server_Link = "https://discord.gg/inccel";
        readonly string strawberrySoftwareLink = "https://discord.gg/9EAGVZUt2Y";
        dynamic jsonData;
        readonly Random rand = new();
        readonly System.ComponentModel.ComponentResourceManager resources = new(typeof(Form1));
        Dictionary<string, RichTextBox> dataBindings = [];
        string notes = "";

        public Form1()
        {
            InitializeComponent();
            ChangeLanguage("en"); // PopulateThemeMenu(); is located in here.
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

        public class Theme
        {
            public string NameEN { get; set; }
            public string NameDE { get; set; }
            public string NameZH { get; set; }
            public string NameJA { get; set; }
            public string NameES { get; set; }
            public string NamePT { get; set; }
            public int BGA { get; set; }
            public int BGR { get; set; }
            public int BGG { get; set; }
            public int BGB { get; set; }
            public int FGA { get; set; }
            public int FGR { get; set; }
            public int FGG { get; set; }
            public int FGB { get; set; }

            /// <summary>
            /// Gets the Foreground Color as a Color class
            /// </summary>
            /// <returns>Color</returns>
            public Color GetForegroundColor()
            {
                return Color.FromArgb(FGA, FGR, FGG, FGB);
            }

            /// <summary>
            /// Gets the Background Color as a Color class
            /// </summary>
            /// <returns>Color</returns>
            public Color GetBackgroundColor()
            {
                return Color.FromArgb(BGA, BGR, BGG, BGB);
            }

            /// <summary>
            /// Gets the localized Theme name.
            /// </summary>
            /// <param name="languageCode">the language code. e.g. "en" or "de"</param>
            /// <returns>string</returns>
            public string GetLocalizedName(string languageCode)
            {
                return languageCode switch
                {
                    "en" => string.IsNullOrEmpty(NameEN) ? "Unknown" : NameEN,
                    "de" => string.IsNullOrEmpty(NameDE) ? NameEN : NameDE,
                    "zh" => string.IsNullOrEmpty(NameZH) ? NameEN : NameZH,
                    "ja" => string.IsNullOrEmpty(NameJA) ? NameEN : NameJA,
                    "es" => string.IsNullOrEmpty(NameES) ? NameEN : NameES,
                    "pt" => string.IsNullOrEmpty(NamePT) ? NameEN : NamePT,
                    _ => NameEN // Default to English if the language is unsupported
                };
            }

        }

        /// <summary>
        /// Load all Themes inside `.\themes`
        /// </summary>
        /// <returns>List\<Theme\> consisting of all themes.</returns>
        private List<Theme> LoadThemes()
        {
            List<Theme> themes = [];
            string themeDirectory = Path.Combine(Application.StartupPath, "themes");

            if (!Directory.Exists(themeDirectory))
            {
                Directory.CreateDirectory(themeDirectory); // Ensure the "themes" directory exists
                CreateDefaultThemes(themeDirectory); // Create default themes if the directory is empty
            }
            if (!Directory.Exists(Path.Combine(themeDirectory, "Default.mdrg_theme")))
            {
                CreateDefaultThemes(themeDirectory);
            }

            try
            {
                // Get all .mdrg_theme files in the theme directory
                var themeFiles = Directory.GetFiles(themeDirectory, "*.mdrg_theme");

                foreach (var themeFile in themeFiles)
                {
                    string json = File.ReadAllText(themeFile);

                    // Deserialize the JSON array into a list of themes
                    var themeList = JsonConvert.DeserializeObject<List<Theme>>(json);

                    if (themeList != null)
                    {
                        themes.AddRange(themeList); // Add themes from this file to the list
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading theme from directory {themeDirectory}: {ex.Message}");
            }

            return themes;
        }


        /// <summary>
        /// Creates Default Themes in `themeDirectory`.
        /// </summary>
        /// <param name="themeDirectory">Directory of themes.</param>
        private void CreateDefaultThemes(string themeDirectory)
        {
            string defaultThemeFile = Path.Combine(themeDirectory, "Default.mdrg_theme");

            if (File.Exists(defaultThemeFile))
                return; // Avoid overwriting existing themes

            var themes = new List<object>
            {
                new
                {
                    NameEN = "Very Dark",
                    NameDE = "Sehr Dunkel",
                    NameZH = "非常暗的",
                    NameJA = "非常に暗い",
                    NameES = "Muy Oscuro",
                    NamePT = "Muito Escuro",
                    BGA = 255,
                    BGR = 0,
                    BGG = 0,
                    BGB = 0,
                    FGA = 255,
                    FGR = 255,
                    FGG = 255,
                    FGB = 255
                },
                new
                {
                    NameEN = "Dark",
                    NameDE = "Dunkel",
                    NameZH = "黑暗",
                    NameJA = "ダーク",
                    NameES = "Oscuro",
                    NamePT = "Escuro",
                    BGA = 255,
                    BGR = 48,
                    BGG = 48,
                    BGB = 48,
                    FGA = 255,
                    FGR = 220,
                    FGG = 220,
                    FGB = 220
                },
                new
                {
                    NameEN = "Pure Light",
                    NameDE = "Rein Hell",
                    NameZH = "纯亮",
                    NameJA = "ピュアライト",
                    NameES = "Pura Luz",
                    NamePT = "Luz Pura",
                    BGA = 255,
                    BGR = 255,
                    BGG = 255,
                    BGB = 255,
                    FGA = 255,
                    FGR = 0,
                    FGG = 0,
                    FGB = 0
                },
                new
                {
                    NameEN = "Light",
                    NameDE = "Hell",
                    NameZH = "光亮",
                    NameJA = "ライト",
                    NameES = "Claro",
                    NamePT = "Claro",
                    BGA = 255,
                    BGR = 220,
                    BGG = 220,
                    BGB = 220,
                    FGA = 255,
                    FGR = 48,
                    FGG = 48,
                    FGB = 48
                }
            };

            string json = JsonConvert.SerializeObject(themes, Formatting.Indented);
            File.WriteAllText(defaultThemeFile, json);
        }

        private void PopulateThemeMenu()
        {
            themeToolStripMenuItem.DropDownItems.Clear(); // Clear existing items
            List<Theme> themes = LoadThemes();

            foreach (Theme theme in themes)
            {
                ToolStripMenuItem item = new(theme.GetLocalizedName(CultureInfo.CurrentCulture.ToString()))
                {
                    Tag = theme // Store the theme object
                };
                item.Click += ThemeMenuItem_Click;
                themeToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void ThemeMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            Theme selectedTheme = (Theme)clickedItem.Tag;  // Get the theme object

            // Call methods to reset the appearance of the menu items and set the new theme's appearance
            ToolStripMenuItem parentItem = clickedItem.OwnerItem as ToolStripMenuItem;
            ResetMenuItem(parentItem);
            SetMenuItemAppearanceBoldChecked(clickedItem);

            // Set the app's background and foreground colors using the selected theme's colors
            SetAppColors(ActiveForm, selectedTheme.GetBackgroundColor(), selectedTheme.GetForegroundColor());
        }


        /// <summary>
        /// Recursively sets the colors for the entire Application.
        /// </summary>
        /// <param name="parent">The parent Application. Usually a Form.</param>
        /// <param name="backColor">Background Color as ARGB. Use `Color.FromArgb(alpha, red, green, blue)`.</param>
        /// <param name="foreColor">Text/Foreground Color as ARGB. Use `Color.FromArgb(alpha, red, green, blue)`.</param>
        private void SetAppColors(Control parent, Color backColor, Color foreColor)
        {

            this.BackColor = backColor;
            this.ForeColor = foreColor;

            foreach (Control control in parent.Controls)
            {
                control.BackColor = backColor;
                control.ForeColor = foreColor;

                // Special handling for MenuStrip and ToolStrip
                if (control is MenuStrip menuStrip)
                {
                    menuStrip.BackColor = backColor;
                    menuStrip.ForeColor = foreColor;
                    foreach (ToolStripMenuItem item in menuStrip.Items)
                    {
                        ApplyMenuStripColors(item, backColor, foreColor);
                    }
                }
                else if (control is ToolStrip toolStrip)
                {
                    toolStrip.BackColor = backColor;
                    toolStrip.ForeColor = foreColor;
                }
                else if (control is TabControl tabControl)
                {
                    tabControl.BackColor = backColor;
                    tabControl.ForeColor = foreColor;
                    tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
                    tabControl.DrawItem += (sender, e) => DrawTabItem(sender, e, backColor, foreColor);

                    foreach (TabPage tabPage in tabControl.TabPages)
                    {
                        tabPage.BackColor = backColor;
                        tabPage.ForeColor = foreColor;

                        // Recursively update all controls inside the TabPage
                        SetAppColors(tabPage, backColor, foreColor);
                    }
                }

                // Recursive call for nested controls
                if (control.HasChildren)
                {
                    SetAppColors(control, backColor, foreColor);
                }
            }

        }

        /// <summary>
        /// Recursively sets the colors for menuItems. Usually called in a foreach loop.
        /// </summary>
        /// <param name="menuItem">The ToolStripMenuItem</param>
        /// <param name="backColor">The background color as ARGB. Use `Color.FromArgb(alpha, red, green, blue)`.</param>
        /// <param name="foreColor">The text/foreground color as ARGB. Use `Color.FromArgb(alpha, red, green, blue)`.</param>
        private void ApplyMenuStripColors(ToolStripMenuItem menuItem, Color backColor, Color foreColor)
        {
            menuItem.BackColor = backColor;
            menuItem.ForeColor = foreColor;

            foreach (ToolStripItem subItem in menuItem.DropDownItems)
            {
                if (subItem is ToolStripMenuItem subMenuItem)
                {
                    ApplyMenuStripColors(subMenuItem, backColor, foreColor);
                }
                else
                {
                    subItem.BackColor = backColor;
                    subItem.ForeColor = foreColor;
                }
            }
        }

        /// <summary>
        /// Customizes the appearance of a TabControl's tab items by applying the specified background and foreground colors.
        /// This method is intended to be used with the DrawItem event of a TabControl set to OwnerDrawFixed.
        /// </summary>
        /// <param name="sender">The TabControl that triggered the DrawItem event.</param>
        /// <param name="e">Provides data for the DrawItem event, including the index of the tab being drawn.</param>
        /// <param name="backColor">The background color to apply to the tab.</param>
        /// <param name="foreColor">The foreground (text) color to apply to the tab label.</param>
        private void DrawTabItem(object sender, DrawItemEventArgs e, Color backColor, Color foreColor)
        {
            TabControl tabControl = sender as TabControl;
            TabPage tabPage = tabControl.TabPages[e.Index];
            Rectangle tabRect = tabControl.GetTabRect(e.Index);

            using SolidBrush backBrush = new(backColor);
            using SolidBrush foreBrush = new(foreColor);

            e.Graphics.FillRectangle(backBrush, tabRect);
            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, tabRect, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        /// <summary>
        /// Changes the Language of the program, and re-initializes the Form.
        /// </summary>
        /// <param name="cultureCode">Culture code, e.g. "de" or "zh".</param>
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

            /// I have no idea why the hell I have to re-set this image, it does NOT
            /// work when I just add it in the visual editor...
            pictureBox1.Image = Properties.Resources.logo;
            /// Continuing after this horrendous failure...

            if (languageMappings.TryGetValue(cultureCode, out var menuItem))
            {
                SetMenuItemAppearanceBoldChecked(menuItem);
                if (cultureCode != "en" && cultureCode != "de" && cultureCode != "zh")
                {
                    MachineLanguageNotice();
                }
            }
            else
            {
                SetMenuItemAppearanceBoldChecked(englishToolStripMenuItem);
            }


            PopulateThemeMenu();
        }

        /// <summary>
        /// Sets a menuItems text to Bold.
        /// </summary>
        /// <param name="menuItem"></param>
        private void SetMenuItemAppearanceBoldChecked(ToolStripMenuItem menuItem)
        {
            menuItem.Checked = true;
            menuItem.Font = new System.Drawing.Font(
                menuItem.Font.FontFamily,
                menuItem.Font.Size,
                System.Drawing.FontStyle.Bold
                );
        }

        /// <summary>
        /// Displays a notice that the language selected was translated using machines and/or AI.
        /// </summary>
        public static void MachineLanguageNotice()
        {
            MessageBox.Show(
                caption: Strings.MachineLanguageNoticeCaption,
                text: Strings.MachineLanguageNoticeText,
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Information
                );
        }

        /// <summary>
        /// Opens a website.
        /// </summary>
        /// <param name="url">The website URL.</param>
        public static void OpenWebsite(string url) => Process.Start("cmd", $"/C start {url}");

        /// <summary>
        /// Adds as many RadioButtons as there are files.
        /// </summary>
        /// <param name="numberOfFiles">How many RadioButtons to create. E.g. 7.</param>
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

        /// <summary>
        /// Manages the selection of the save file upon checking a RadioButton.
        /// </summary>
        /// <param name="sender">Itself</param>
        /// <param name="e">Sent along the EventHandler?</param>
        /// <example>radioButton.CheckedChanged += new EventHandler(RadioButton_CheckedChanged);</example>
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


        public void LoadToolStripMenuItem_Click(object sender, EventArgs e) // On clicking "Load File"
        {
            OpenFileDialog openFileDialog = new()
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
                List<int> slots = [];
                foreach (var save in saves)
                {
                    slots.Add((int)save.slot);
                }
                int slotsAmount = saves.Count;
                AddRadioButtons(slotsAmount);

                debugTextBox.Text = fileContent; // Pump entire JSon into Debug

            }
        }

        /// <summary>
        /// Reloads and updates the values displayed in the form based on the selected save file.
        /// This method parses the save data and achievements from the JSON file, updates the UI elements,
        /// and handles any errors that may occur during the process.
        /// </summary>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Parses the save data and achievements from the JSON file.
        /// 2. Updates the UI elements with the parsed data.
        /// 3. Converts raw time values into formatted strings.
        /// 4. Maps rent status text to corresponding weekdays.
        /// 5. Updates checkboxes and other controls based on the parsed data.
        /// 6. Handles various exceptions and displays appropriate error messages.
        /// </remarks>
        /// <exception cref="System.OverflowException">Thrown when an overflow occurs during data processing.</exception>
        /// <exception cref="System.NullReferenceException">Thrown when a null reference is encountered during data processing.</exception>
        /// <exception cref="Exception">Thrown when an unknown error occurs during data processing.</exception>
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
                    { "_uniqueConversationsLeft", uniqueConversationsLeftTextBox },
                    { "_currentHorniness", currentHorninessTextBox },
                    { "_satiation", satiationTextBox },
                    { "weeklyRent", weeklyRentTextBox },
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
                notes = saveFileJson["saves"][selectedSaveFile].Value<string>("notes");
                notesTextBox.Text = notes;

                JArray jsVisitedWebsites = (JArray)saveFileJson["visitedWebsites"];
                JArray jsGottenAchievements = (JArray)achievementsObject["values"];
                string[] visitedWebsites = jsVisitedWebsites.ToObject<string[]>();
                string[] gottenAchievements = jsGottenAchievements.ToObject<string[]>();

                // Continue various variables
                bool lightSwitchOn = savedataObject.Value<bool>("lightSwitchOn");

                // Convert raw time into days, hours, and minutes
                const int MPD = 24 * 60, MPH = 60;
                int streamTime = int.Parse(totalStreamTimeRawBox.Text);
                int longestStream = int.Parse(longestStreamRawBox.Text);

                int streamDays = streamTime / MPD, streamHours = (streamTime % MPD) / MPH, streamMinutes = streamTime % MPH;
                int longestStreamDays = longestStream / MPD, longestStreamHours = (longestStream % MPD) / MPH, longestStreamMinutes = longestStream % MPH;
                int ingameTimeDays = ingameTime / MPD, ingameTimeHours = (ingameTime % MPD) / MPH, ingameTimeMinutes = ingameTime % MPH;


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

                weekdayTextBox.Text = rentDaysMapping.TryGetValue(savedataObject["statusText"].ToString(), out var day)
                    ? day
                    : Strings.WeekDaysUnknown;

                // Quick Checkbox assignment
                lightswitchCheckbox.Checked = lightSwitchOn;

                // Get a list of items to check without modifying the original collection while iterating
                var itemsToCheck = websitesCheckBoxes.Items.Cast<string>()
                    .Where(visitedWebsites.Contains)
                    .ToList();

                // Now modify the control safely
                foreach (var item in itemsToCheck)
                {
                    websitesCheckBoxes.SetItemChecked(websitesCheckBoxes.Items.IndexOf(item), true);
                }

                // Gotten Achievements checker
                foreach (Control groupBox in achievementsPanel.Controls)
                {
                    if (groupBox is GroupBox currentGroupBox)
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

        /// <summary>
        /// Shows a popup about updates.
        /// </summary>
        /// <param name="isNewest">Is the app at its latest version?</param>
        /// <param name="current">The current version.</param>
        /// <param name="latest">The latest version on GitHub.</param>
        /// <param name="isStartup">Was this called at Startup?</param>
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

        private void CheckForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckForUpdate(false);
        }

        /// <summary>
        /// Checks for updates.
        /// </summary>
        /// <param name="isStartup">Is this called at Startup?</param>
        private async void CheckForUpdate(bool isStartup)
        {

            string owner = "Wehrmachtserdbeere";
            string repo = "MDRG-Analyzer";

            using HttpClient thisProgram = new();
            try
            {
                thisProgram.DefaultRequestHeaders.Add("User-Agent", "request"); // Required for GitHub API

                HttpResponseMessage gitResponse = await thisProgram.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/latest"); // Get the API URL

                if (gitResponse.IsSuccessStatusCode)
                {
                    string gitLatestVersion = JsonConvert.DeserializeObject<JObject>(
                        await gitResponse.Content.ReadAsStringAsync()
                    )?["tag_name"]?.ToString();

                    if (gitLatestVersion != null &&
                        Version.TryParse(gitLatestVersion.TrimStart('v'), out Version latest) &&
                        Version.TryParse(__version__, out Version current))
                    {
                        ShowUpdatePopup(current.CompareTo(latest) >= 0, current, latest, isStartup);
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


        // Quickly invert achievementsPanel and visitedWebsiteGrouBox visibility
        private void Button1_Click(object sender, EventArgs e) => achievementsPanel.Visible = !achievementsPanel.Visible;
        private void Button2_Click(object sender, EventArgs e) => visitedWebsiteGroupBox.Visible = !visitedWebsiteGroupBox.Visible;


        private void ReportABugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebsite(repoUrl + "/issues");
        }

        private void SuggestAFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebsite(repoUrl + "/discussions/categories/ideas");
        }

        private void QuickLinkToGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebsite(repoUrl);
        }

        private void OpenReadmeToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void EditInfoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                caption: Strings.EditInfoButtonCaption,
                text: Strings.EditInfoButtonText
                );
        }

        private void CreateBackupButton_Click(object sender, EventArgs e)
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


        private void SaveEditConsentBox_CheckedChanged(object sender, EventArgs e)
        {
            ControlExtensions.ToggleControlsEnabled(groupBox3); // General Info
            ControlExtensions.ToggleControlsEnabled(groupBox15); // Bot Info
            ControlExtensions.ToggleControlsEnabled(groupBox25); // Misc Info
            ControlExtensions.ToggleControlsEnabled(saveEditGroupBox); // Save Edit Box
            ControlExtensions.ToggleControlsEnabled(groupBox4); // Church Achievement Box
            ControlExtensions.ToggleControlsEnabled(groupBox5); // Misc Achievement Box
            websitesCheckBoxes.Enabled = !websitesCheckBoxes.Enabled; // Websites Visited Box

        }

        private void GuideButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                caption: Strings.GuideAndInstructionsCaption,
                text: Strings.GuideAndInstructionsText
                );
        }

        private void SaveButton_Click(object sender, EventArgs e)
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

                    List<string> achievementsSelected = [];

                    foreach (Object item in checkedListBox1.CheckedItems)
                    {
                        achievementsSelected.Add($"{item}");
                    }
                    foreach (Object item in checkedListBox2.CheckedItems)
                    {
                        achievementsSelected.Add($"{item}");
                    }

                    JArray achievementsSelectedJArray = new(achievementsSelected);
                    
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

                    List<string> visitedWebsites = [];

                    foreach (var item in websitesCheckBoxes.CheckedItems)
                    {
                        visitedWebsites.Add(item.ToString());
                    }

                    JArray visitedWebsitesJArray = new(visitedWebsites);

                    try
                    {
                        string updatedSavedataJson = savedataObject.ToString();
                        saveFileJson["saves"][selectedSaveFile]["savedata"] = updatedSavedataJson;
                        saveFileJson["saves"][selectedSaveFile]["notes"] = notesTextBox.Text;
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

        private void ExtraCreditsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                caption: Strings.ExtraCreditsCaption,
                text: Strings.ExtraCreditsText
                );
        }

        private void DonateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebsite("https://ko-fi.com/strawberrysoftware");
        }

        private void PictureBox1_Click(object sender, EventArgs e)
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

        /// <summary>
        /// Resets a ToolStripMenuItem to a default state. This means its previous size and Font Family, but without Bold or Italics, and not Checked.
        /// </summary>
        /// <param name="item">The ToolStripMenuItem to be reset.</param>
        private void ResetMenuItem(ToolStripMenuItem item)
        {
            if (item.HasDropDownItems)
            {
                foreach (ToolStripMenuItem itemItem in item.DropDownItems)
                {
                    ResetMenuItem(itemItem);
                }
            }
            else
            {
                item.Checked = false;
                item.Font = new System.Drawing.Font(
                    item.Font.FontFamily,
                    item.Font.Size,
                    System.Drawing.FontStyle.Bold
                );
            }
        }

        private void MDRGDiscordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebsite(MDRG_Server_Link);
        }

        private void StrawberrySoftwareDiscordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebsite(strawberrySoftwareLink);
        }
    }
}

public static class ControlExtensions
{
    /// <summary>
    /// Toggles the enabled state of controls within a GroupBox.
    /// This method recursively processes all child controls within the specified GroupBox,
    /// toggling their enabled state or read-only state as appropriate.
    /// </summary>
    /// <param name="control">The GroupBox control whose child controls' states are to be toggled.</param>
    /// <remarks>
    /// This method performs the following actions:
    /// 1. Toggles the ReadOnly property of RichTextBox and TextBox controls, excluding specific named controls.
    /// 2. Toggles the Enabled property of Button, CheckBox, and CheckedListBox controls.
    /// 3. Recursively processes nested GroupBox controls.
    /// </remarks>
    public static void ToggleControlsEnabled(this Control control)
    {
        if (control is GroupBox groupBox)
        {
            foreach (Control childControl in groupBox.Controls)
            {
                switch (childControl)
                {
                    case RichTextBox richTextBox when richTextBox.Name != "weekdayTextBox" && richTextBox.Name != "rentTextBox":
                        richTextBox.ReadOnly = !richTextBox.ReadOnly;
                        break;
                    case TextBox textBox when textBox.Name != "weekdayTextBox" && textBox.Name != "rentTextBox":
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