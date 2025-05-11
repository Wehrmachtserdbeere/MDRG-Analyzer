using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using MDRG_Analyzer_Multi_Platform.Resources;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MDRG_Analyzer
{
    public class Program
    {
        string fileContent;
        JObject saveFileJson;
        dynamic jsonData;

        public static void Main()
        {
            var app = new Program();
            Program.Run();
        }

        public static void Run()
        {

            // Create Settings if it does not exist yet.

            if (!File.Exists("Settings.json"))
            {
                File.Create("Settings.json").Close();
            }

            var __version__ = Assembly.GetExecutingAssembly().GetName().Version;
            string settingsJson = File.ReadAllText("Settings.json");
            var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsJson);


            // Get default language from settings. If not found, set to English.
            string defaultLanguage = settings != null && settings.ContainsKey("DefaultLanguage") ? settings["DefaultLanguage"] : "en-US";

            // Welcome, here's the logo.
            Console.WriteLine(
            " ▒▒███▒▒███▒▒█████▒▒█████▒▒▒████▒▒▒▒▒▒▒▒▒\n" +
            "░▒██▒▒██▒▒██▒██▒▒██▒██▒▒██▒██▒▒▒█▒▒▒▒▒▒▒▒\n" +
            "░▒██▒▒██▒▒██▒██▒▒██▒██▒██▒▒██▒█▒▒▒▒▒▒▒▒▒▒\n" +
            "░▒██▒▒▒▒▒▒██▒██▒▒██▒█████▒▒██▒▒██▒▒▒▒▒▒▒▒\n" +
            "░▒██▒▒▒▒▒▒██▒█████▒▒██▒▒██▒▒█████▒▒▒▒▒▒▒▒\n" +
            "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ \n" +
            " ▒▒█▒▒██▒▒█▒▒█▒▒█▒▒█▒█▒█████▒████▒███▒▒▒▒\n" +
            "░▒█▒█▒█▒█▒█▒█▒█▒█▒▒█▒█▒▒▒▒█▒▒█▒▒▒▒█▒▒█▒▒▒\n" +
            "░▒███▒█▒█▒█▒███▒█▒▒▒█▒▒▒▒█▒▒▒████▒███▒▒▒▒\n" +
            "░▒█▒█▒█▒█▒█▒█▒█▒█▒▒▒█▒▒▒█▒▒▒▒█▒▒▒▒█▒▒█▒▒▒\n" +
            "░▒█▒█▒█▒▒██▒█▒█▒███▒█▒▒█████▒████▒█▒▒█▒▒▒\n" +
            "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ \n" +
            $"\t\t\t{Generic.version} {__version__}\n"
            );

            Console.WriteLine();
            Console.WriteLine(Generic.welcome);
        }

        /// <summary>
        /// Opens a website in the default browser.
        /// </summary>
        /// <param name="url">URL to be opened.</param>
        public void OpenWebsite(string url) => Process.Start("cmd", $"/c start {url}");

        public void MainMenu(bool clearConsole = true)
        {
            if (clearConsole)
            {
                Console.Clear();
            }

            Console.WriteLine($" -- {Generic.mainmenu} --\n");

            Console.WriteLine(
                $"(1) {Generic.loadfile}\n" +
                $"(2) {Generic.settings}"
                );
            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();

            switch (key.Key)
            {
                case ConsoleKey.D1:
                    LoadFile();
                    break;
                case ConsoleKey.D2:
                    OpenSettings();
                    break;
                default:
                    Console.WriteLine(Generic.invalidkey);
                    MainMenu();
                    break;
            }
        }

        public void LoadFile(bool clearConsole = true)
        {
            if (clearConsole)
            {
                Console.Clear();
            }
            Console.WriteLine($"-- {Generic.loadfile} --");
            Console.WriteLine();
            Console.WriteLine(Generic.loadfile_instructions);

            string savefile = String.Empty;

            Console.WriteLine();
            string input = Console.ReadLine();
            Console.WriteLine();

            // Cover common shortcuts
            if (input.ToLower() == "documents")
            {
                input = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else if (input.ToLower() == "desktop")
            {
                input = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else if (input.ToLower() == "downloads")
            {
                input = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            }

            // Did the user input a file or a directory?
            if (File.Exists(input))
            {
                // Does the file end with .mdrg? If not, tell user to try again.
                if (!input.EndsWith(".mdrg"))
                {
                    Console.WriteLine(Generic.loadfile_invalid);
                    LoadFile(false);
                    return;
                }

                // Save file as System Path to send later
                savefile = Path.GetFullPath(input);
            }
            else if (Directory.Exists(input))
            {
                // Are there any files here that end with .mdrg? There *is* a possibility for an integer overflow here, but if someone has more than 2 billion files in a directory, they don't deserve to use this program.
                string[] files = Directory.GetFiles(input, "*.mdrg");
                if (files.Length == 0)
                {
                    Console.WriteLine(Generic.loadfile_invalid);
                    LoadFile(false);
                    return;
                }

                // If there are, list all with a number and ask user to select one.
                Console.WriteLine(Generic.loadfile_selectoneofthesefiles);
                for (int i = 0; i < files.Length; i++)
                {
                    Console.WriteLine($"({i + 1}) {files[i]}");
                }
                Console.WriteLine($"{Generic.input}:");
                // ReadLine because there might be more than 9 files.
                string fileInput = Console.ReadLine();

                // Check if the input is a number and within the range of files
                if (int.TryParse(fileInput, out int fileIndex) && fileIndex > 0 && fileIndex <= files.Length)
                {
                    // -1 because we need to account for the zero-based index
                    savefile = Path.GetFullPath(files[fileIndex - 1]);
                }
                else
                {
                    Console.WriteLine(Generic.invalidkey);
                    LoadFile(false);
                    return;
                }
            }
            else
            {
                Console.WriteLine(Generic.loadfile_invalid);
                LoadFile(false);
            }

            // If we have a file, continue.
            if (savefile != null)
            {
                ProcessFile(savefile);
            }
        }

        /// <summary>
        /// Processes the file and creates lists of the values.
        /// </summary>
        /// <param name="filePath"></param>
        public void ProcessFile(string filePath, bool clearConsole = true)
        {
            if (clearConsole)
            {
                Console.Clear();
            }

            fileContent = File.ReadAllText(filePath);
            saveFileJson = JObject.Parse(fileContent);

            // Select save file
            int selectedSaveFile = SelectSaveFile(filePath);

            if (selectedSaveFile == -1) {
                // This certainly should not happen.
                Console.WriteLine(Generic.selectsaveslot_invalidsaveslot);
                ProcessFile(filePath, false);
            }

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
                    // No savedata found, show error and specifically detail to the user that no, they cannot load an empty save file, and that they have to export it from the game FIRST.
                    Console.WriteLine(Generic.nosavedata);
                }
                else
                {
                    // Something else happened, please contact me.
                    Console.WriteLine(Generic.pleasecontactme);
                }
            }

            // Prepare three lists
            List<StringValue> stringValues = new List<StringValue>();
            List<IntValue> intValues = new List<IntValue>();
            List<DoubleValue> doubleValues = new List<DoubleValue>();

            // Now that we have the savedata, we can create a dictionary, following the schema of InternalID, ID, and Data Type
            Dictionary<(string, string), Type> dataBindings = new Dictionary<(string, string), Type>
            {
                { ( "botName", "data_botname"), typeof(string) },
                { ( "money", "data_money"), typeof(int) },
                { ( "gameVersion", "data_gameversion"), typeof(string) },
                { ( "playerName", "data_playername"), typeof(string) },
                { ( "casinoTokens", "data_casinotokens"), typeof(int) },
                { ( "_lust", "data_lust"), typeof(double) },
                { ( "_sympathy", "data_sympathy"), typeof(double) },
                { ( "_longing", "data_longing"), typeof(double) },
                { ( "inteligence", "data_inteligence"), typeof(double) },
                { ( "stage", "data_stage"), typeof(int) },
                { ( "subs", "data_subs"), typeof(int) },
                { ( "followers", "data_followers"), typeof(int) },
                { ( "streamedFor", "data_streamedfor"), typeof(int) },
                { ( "moneyEarnedFromDonations", "data_moneyearnedfromstreams"), typeof(int) },
                { ( "longestStream", "data_longeststream"), typeof(int) },
                { ( "timesCameInside", "data_timescameinside"), typeof(int) },
                { ( "timesCameInsideAnal", "data_timescameinsideanal"), typeof(int) },
                { ( "timesCameInMouth", "data_timescameinsidemouth"), typeof(int) },
                { ( "timesCameOutside", "data_timescameoutside"), typeof(int) },
                { ( "streamCount", "data_streamcount"), typeof(int) },
                { ( "timesLostChess", "data_timeslostchess"), typeof(int) },
                { ( "timesWonChess", "data_timeswonchess"), typeof(int) },
                { ( "timesLostOldMaid", "data_timelostoldmaid"), typeof(int) },
                { ( "timesWonOldMaid", "data_timeswonoldmaid"), typeof(int) },
                { ( "timesRanAwayOldMaid", "data_timesranawayoldmaid"), typeof(int) },
                { ( "timesLostWordChain", "data_timeslostwordchain"), typeof(int) },
                { ( "timesWonWordChain", "data_timeswonwordchain"), typeof(int) },
                { ( "vinegaraEffectEnd", "data_vinegaraend"), typeof(double) }, // Is it double?
                { ( "deathGripEffectEnd", "data_deathgripend"), typeof(double) }, // ?
                { ( "timesWentToChurch", "data_timeswenttochurch"), typeof(int) },
                { ( "lastMentalHealthInfoAt", "data_lastmentalhealthinfoat"), typeof(int) },
                { ( "lastHungerInfoAt", "data_lasthungerinfoat"), typeof(int) },
                { ( "lastHeadpatedAt", "data_lastheadpatat"), typeof(int) },
                { ( "lastBotStartedTalkAt", "data_lastbottalkat"), typeof(int) },
                { ( "lastStreamedAt", "data_laststreamedat"), typeof(int) },
                { ( "lastOutsideWithBotAt", "data_lastoutsidewithbot"), typeof(int) },
                { ( "lastEquipmentAt", "data_lastequipmentat"), typeof(int) },
                { ( "lastInteractAt", "data_lastinteractat"), typeof(int) },
                { ( "lastFuckedAt", "data_lastfuckedat"), typeof(int) },
                { ( "lastWokeUpAt", "data_lastwokeupat"), typeof(int) },
                { ( "lastWentToChurchAt", "data_lastwenttochurchat"), typeof(int) },
                { ( "lastWorkedAtDay", "data_lastworkedatday"), typeof(int) },
                { ( "nunPoints", "data_nunpoints"), typeof(int) },
                { ( "priestBotPoints", "data_priestbotpoints"), typeof(int) },
                { ( "lastCuddledAt", "data_lastcuddledat"), typeof(int) },
                { ( "_maxCum", "data_maxcum"), typeof(double) },
                { ( "_remainingCum", "data_remainingcum"), typeof(double) },
                { ( "_health", "data_health"), typeof(double) },
                { ( "_stamina", "data_stamina"), typeof(double) },
                { ( "_mentalHealth", "data_mentalhealth"), typeof(double) },
                { ( "_mood", "data_mood"), typeof(double) },
                { ( "_cumInside", "data_cuminside"), typeof(double) },
                { ( "_cumInsideAnal", "data_cuminsideanal"), typeof(double) },
                { ( "_cumInsideStomach", "data_cuminsidestomach"), typeof(double) },
                { ( "mlCameInMouth", "data_mlcameinmouth"), typeof(double) },
                { ( "mlOfCumWasted", "data_mlofcumwasted"), typeof(double) },
                { ( "search", "data_search"), typeof(int) },
                { ( "_uniqueConversationsLeft", "data_uniqueconversationsleft"), typeof(int) },
                { ( "_currentHorniness", "data_currenthorniness"), typeof(double) },
                { ( "_satiation", "data_satiation"), typeof(double) },
                { ( "weeklyRent", "data_weeklyrent"), typeof(string) }, // ???
            };

            // Depending on the type (Value), create one of the custom classes.
            foreach (var entry in dataBindings)
            {
                if (entry.Value == typeof(string))
                {
                    stringValues.Add(new StringValue
                    {
                        ID = entry.Key.Item2,
                        Value = savedataObject.Value<string>(entry.Key.Item1).ToString(),
                        InternalID = entry.Key.Item1
                    });
                }
                else if (entry.Value == typeof(int))
                {
                    intValues.Add(new IntValue
                    {
                        ID = entry.Key.Item2,
                        Value = savedataObject.Value<int>(entry.Key.Item1),
                        InternalID = entry.Key.Item1
                    });
                }
                else if (entry.Value == typeof(double))
                {
                    doubleValues.Add(new DoubleValue
                    {
                        ID = entry.Key.Item2,
                        Value = savedataObject.Value<double>(entry.Key.Item1),
                        InternalID = entry.Key.Item1
                    });
                }
            }

            int saveSlot = saveFileJson["saves"][selectedSaveFile].Value<int>("slot");
            int ingameTime = saveFileJson["saves"][selectedSaveFile].Value<int>("ingameTime");
            string notes = saveFileJson["saves"][selectedSaveFile].Value<string>("notes");

            JArray jsVisitedWebsites = (JArray)saveFileJson["visitedWebsites"];
            JArray jsGottenAchievements = (JArray)achievementsObject["values"];
            string[] visitedWebsites = jsVisitedWebsites.ToObject<string[]>();
            string[] gottenAchievements = jsGottenAchievements.ToObject<string[]>();

            // Continue various variables
            bool lightSwitchOn = savedataObject.Value<bool>("lightSwitchOn");

            // Convert raw time into days, hours, and minutes
            const int MPD = 24 * 60, MPH = 60;
            int streamTime = int.Parse(savedataObject.Value<int>("streamedFor").ToString());
            int longestStream = int.Parse(savedataObject.Value<int>("longestStream").ToString());

            int streamDays = streamTime / MPD, streamHours = (streamTime % MPD) / MPH, streamMinutes = (streamTime % MPD) % MPH;
            int longestStreamDays = longestStream / MPD, longestStreamHours = (longestStream % MPD) / MPH, longestStreamMinutes = longestStream % MPH;
            int ingameTimeDays = ingameTime / MPD, ingameTimeHours = (ingameTime % MPD) / MPH, ingameTimeMinutes = ingameTime % MPH;

            var rentDaysMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "rent today!", Generic.WeekDaysMonday },
                    { "rent in 1 day", Generic.WeekDaysSunday },
                    { "rent in 2 days", Generic.WeekDaysSaturday },
                    { "rent in 3 days", Generic.WeekDaysFriday },
                    { "rent in 4 days", Generic.WeekDaysThursday },
                    { "rent in 5 days", Generic.WeekDaysWednesday },
                    { "rent in 6 days", Generic.WeekDaysTuesday },
                    { "rent in 7 days", Generic.WeekDaysMonday }
                };

            string weekdayText = rentDaysMapping.TryGetValue(savedataObject["statusText"].ToString(), out var day)
                    ? day
                    : Generic.WeekDaysUnknown;

            // Data finished assigning.
            // TODO: Continue to the showing of all the data. Stuff like a foreach loop that checks if the value is a string, int, or double, and then prints out the data, all in a nice format with numbers for later save editing.
        }

        public int SelectSaveFile(string filePath)
        {
            // Check how many non-autosaves there are
            // Load save file into variable, then parse the JSon
            string fileContent = File.ReadAllText(filePath);
            JObject saveFileJson = JObject.Parse(fileContent);

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

            // Ask user to select a save file
            Console.WriteLine(Generic.selectsavefile);
            for (int i = 0; i < slotsAmount; i++)
            {
                Console.WriteLine($"({i + 1}) {Generic.slot} {slots[i]}");
            }
            Console.WriteLine($"{Generic.input}:");
            string input = Console.ReadLine();
            // Quick try-convert to int
            if (int.TryParse(input, out int selectedSaveFile))
            {
                // Check if the input is a number and within the range of saves
                if (selectedSaveFile > 0 && selectedSaveFile <= slotsAmount)
                {
                    // -1 because we need to account for the zero-based index
                    return selectedSaveFile - 1;
                }
                else
                {
                    Console.WriteLine(Generic.invalidkey);
                }
            }
            else
            {
                Console.WriteLine(Generic.invalidkey);
            }
            SelectSaveFile(filePath);
            return -1; // This should never happen, but just in case. And so VS stops screaming at me.
        }

        /// <summary>
        /// Opens the settings menu.
        /// </summary>
        public void OpenSettings(bool clearConsole = true)
        {
            if (clearConsole)
            {
                Console.Clear();
            }

            Console.WriteLine($" -- {Generic.settings} --");

            // First off, do we have a settings file? Redundant, but just in case the user deletes it while the program runs.
            if (!File.Exists("Settings.json"))
            {
                File.Create("Settings.json").Close();
            }

            // Load settings from JSON file using Newtonsoft.Json
            string json = File.ReadAllText("Settings.json");
            var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            // If default language is not found, set it to English.
            if (settings is null || settings["DefaultLanguage"] is null)
            {
                // If settings, which should be a var JsonConvert.DeserializeObject is null, create a new dictionary.
                settings = new Dictionary<string, string>();
                // If DefaultLanguage is null, create it.
                settings["DefaultLanguage"] = "en-US";
                // Save settings back to JSON file
                json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText("Settings.json", json);
            }


            Console.WriteLine(
                $"\n{Generic.settings_instructions}\n" +
                $"(1) {Generic.settings_defaultlanguage}: {new CultureInfo(settings["DefaultLanguage"]).DisplayName}\n" +
                $"(E) {Generic.exit}"
            );

            ConsoleKeyInfo key = Console.ReadKey();
            Console.Clear();
            switch (key.Key)
            {
                // Default Language
                case ConsoleKey.D1:
                    Console.WriteLine($" -- {Generic.settings_defaultlanguage} --");
                    Console.WriteLine();
                    Console.WriteLine(
                        $"{Generic.settings_defaultlanguage_prompt}\n" +
                        $"{Generic.settings_availablelanguages}:"
                        );

                    LocalizationHelper locHelper = new();

                    // List available languages
                    List<CultureInfo> availableCultures = locHelper.GetAvailableCultures();
                    int i = 2; // Int because there aren't enough languages to overflow. I hope.
                    // Always display English as the first option.
                    Console.WriteLine($"\t(1) {CultureInfo.GetCultureInfo("en-US").Name} - {CultureInfo.GetCultureInfo("en-US").EnglishName}");
                    foreach (var culture in availableCultures)
                    {
                        Console.WriteLine($"\t({i}) {culture.Name} - {culture.EnglishName}");
                        i++;
                    }
                    ConsoleKeyInfo cultureKey = Console.ReadKey();
                    Console.WriteLine();
                    // Choose language based on input
                    if (cultureKey.KeyChar == '1')
                    {
                        settings["DefaultLanguage"] = "en-US";
                        CultureInfo.CurrentCulture = new CultureInfo("en-US");
                        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
                    }
                    // Check if the input is a number and within the range of available cultures
                    else if (int.TryParse(cultureKey.KeyChar.ToString(), out int cultureIndex) && cultureIndex > 1 && cultureIndex <= availableCultures.Count + 1)
                    {
                        // -2 because we need to account for the English option and the zero-based index
                        var selectedCulture = availableCultures[cultureIndex - 2];
                        settings["DefaultLanguage"] = selectedCulture.Name;
                        CultureInfo.CurrentCulture = selectedCulture;
                        CultureInfo.CurrentUICulture = selectedCulture;
                        Console.WriteLine($"{Generic.settings_youhaveselectedlanguage}: {selectedCulture.Name} - {selectedCulture.EnglishName}");
                    }
                    else
                    {
                        Console.WriteLine($"{Generic.invalidkey}");
                        settings["DefaultLanguage"] = "en-US";
                        CultureInfo.CurrentCulture = new CultureInfo("en-US");
                        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
                    }
                    // Save settings back to JSON file
                    json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText("Settings.json", json);
                    OpenSettings();
                    break;
                case ConsoleKey.E:
                    return;
                default:
                    Console.WriteLine(Generic.invalidkey);
                    OpenSettings();
                    break;
            }
        }

    }

    /// <summary>
    /// Helper class for localization. Mainly for in-class use.
    /// </summary>
    public class LocalizationHelper
    {
        private readonly ResourceManager _rm = IDs.ResourceManager;

        /// <summary>
        /// Gets the localized name for a given ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetLocalizedName(string id)
        {
            var culture = CultureInfo.CurrentCulture;

            // Try with current culture
            string localized = _rm.GetString(id, culture);

            // Fallback to default if not found
            if (string.IsNullOrEmpty(localized))
            {
                localized = _rm.GetString(id, CultureInfo.InvariantCulture);
            }

            // Fallback to ID if missing
            return localized ?? id;
        }

        /// <summary>
        /// Gets a list of all cultures that have localization files.
        /// </summary>
        /// <returns></returns>
        public List<CultureInfo> GetAvailableCultures()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures);
            var availableCultures = new List<CultureInfo>();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Assembly mainAssembly = Assembly.GetExecutingAssembly();

            foreach (var culture in cultures)
            {
                if (culture.Name == "") continue; // Skip invariant

                string cultureDir = Path.Combine(baseDir, culture.Name);
                string satellitePath = Path.Combine(cultureDir, $"{mainAssembly.GetName().Name}.resources.dll");

                if (File.Exists(satellitePath))
                {
                    availableCultures.Add(culture);
                }
            }

            return availableCultures;
        }
    }

    /// <summary>
    /// Class representing a value with a string. Not to be used for values expecting any number.
    /// </summary>
    public class StringValue
    {
        /// <summary>
        /// Localization key. Must be unique and exist in the `IDs.resx` file.
        /// </summary>
        public required string ID { get; set; }

        /// <summary>
        /// The actual value used in the program. Typically deserialized from JSON.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The unique identifier used to locate this value in external JSON.
        /// </summary>
        public required string InternalID { get; set; }

        /// <summary>
        /// Gets the localized name for this value using its ID.
        /// </summary>
        public string GetName()
        {
            LocalizationHelper locHelper = new();
            return locHelper.GetLocalizedName(ID);
        }
    }


    /// <summary>
    /// Class representing a value with an integer. Not to be used for values expecting any string.
    /// </summary>
    public class IntValue
    {
        /// <summary>
        /// Localization key. Must be unique and exist in the `IDs.resx` file.
        /// </summary>
        public required string ID { get; set; }

        /// <summary>
        /// The actual value used in the program. Typically deserialized from JSON.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The unique identifier used to locate this value in external JSON.
        /// </summary>
        public required string InternalID { get; set; }
        public string GetName()
        {
            LocalizationHelper locHelper = new();
            return locHelper.GetLocalizedName(ID);
        }

        /// <summary>
        /// Attempts to parse a string to an integer. If it fails, throws a FormatException.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="FormatException"></exception>
        public void FromString(string value)
        {
            if (int.TryParse(value, out int result))
            {
                Value = result;
            }
            else
            {
                throw new FormatException($"Invalid format for IntValue: {value}");
            }
        }
    }

    public class DoubleValue
    {
        /// <summary>
        /// Localization key. Must be unique and exist in the `IDs.resx` file.
        /// </summary>
        public required string ID { get; set; }

        /// <summary>
        /// The actual value used in the program. Typically deserialized from JSON.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// The unique identifier used to locate this value in external JSON.
        /// </summary>
        public required string InternalID { get; set; }
        public string GetName()
        {
            LocalizationHelper locHelper = new();
            return locHelper.GetLocalizedName(ID);
        }

        /// <summary>
        /// Attempts to parse a string to a double. If it fails, throws a FormatException.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="FormatException"></exception>
        public void FromString(string value)
        {
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                Value = result;
            }
            else
            {
                throw new FormatException($"Invalid format for DoubleValue: {value}");
            }
        }

        /// <summary>
        /// Clones the given IntValue to this DoubleValue.
        /// </summary>
        /// <param name="intValue"></param>
        public void FromIntValue(IntValue intValue)
        {
            // Ensure intValue is not null
            if (intValue is null)
            {
                throw new ArgumentNullException(nameof(intValue), "IntValue cannot be null");
            }
            // Ensure ID is not null or empty
            if (string.IsNullOrEmpty(intValue.ID))
            {
                throw new ArgumentException("ID cannot be null or empty", nameof(intValue));
            }
            else
            {
                ID = intValue.ID;
            }
            // Ensure Value is not null or empty
            if (string.IsNullOrEmpty(intValue.InternalID))
            {
                throw new ArgumentException("InternalID cannot be null or empty", nameof(intValue));
            }
            else
            {
                InternalID = intValue.InternalID;
            }
            // Since int is never null, we can safely convert it to double without a check.
            Value = intValue.Value;
        }
    }
}