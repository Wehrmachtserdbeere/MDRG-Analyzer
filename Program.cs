using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using MDRG_Analyzer_Multi_Platform.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MDRG_Analyzer
{
    public class Program
    {

        /// TODO - Once the dev decides to stop being a massive pain and uses NORMAL JSON, this will be continued.
        /// Until then, this is just a stat checker without any save editing capabilities.
        /// Fuck this.

        string fileContent;
        JObject savefileJson;
        JObject savefileRoot;
        dynamic jsonData;
        bool createdBackup = false;
        JObject websites = [];
        string[] visitedWebsites = Array.Empty<string>();
        JObject achievementsObject = [];
        string[] gottenAchievements = Array.Empty<string>();
        List<StringValue> stringValues = [];
        List<IntValue> intValues = [];
        List<DoubleValue> doubleValues = [];
        List<BoolValue> boolValues = [];
        string filePath;
        int selectedSaveFile = -1;
        string notes;
        int streamDays = 0, streamHours = 0, streamMinutes = 0,
                longestStreamDays = 0, longestStreamHours = 0, longestStreamMinutes = 0,
                ingameTimeDays = 0, ingameTimeHours = 0, ingameTimeMinutes = 0;
        string weekdayText = string.Empty;
        int saveSlot = 0;

        // List of all achievements
        Dictionary<string, string> allAchievements = new()
        {
            { "church-endSpotkanie2B", Achievements.ending_oblivious },
            { "church-endTheWorstEnd", Achievements.ending_worst },
            { "church-endIdontknow", Achievements.ending_idontknow },
            { "church-endJustGoThereGood", Achievements.ending_goodend2 },
            { "church-endJustGoThereBad", Achievements.ending_notgoodenough },
            { "ending-ballsexploded", Achievements.ending_ballsexploded },
            { "ending-goodEnd1", Achievements.ending_goodend1 },
            { "ending-badEnd1", Achievements.ending_badend1 },
            { "ending-badEnd2", Achievements.ending_badend2 },
            { "ending-badEnd3", Achievements.ending_badend3 },
            { "ending-badEnd4.1", Achievements.ending_badend4_1 },
            { "ending-badEnd4.2", Achievements.ending_badend4_2 },
            { "ending-healthEnd", Achievements.ending_healthend },
            { "ending-fedEnd", Achievements.ending_fedend },
            { "ending-schizoEnd", Achievements.ending_schizoend },
            { "ending-genericSchizoEnd", Achievements.ending_genericschizoend },
            { "ending-KilledByADirtyCop1", Achievements.ending_killedbyadirtycop1 },
            { "ending-KilledByADirtyCop2", Achievements.ending_killedbyadirtycop2 },
            { "ending-theyKnowOblivious", Achievements.ending_theyknowoblivious },
            { "ending-theyKnow", Achievements.ending_theyknow },
            { "bigdaddyhurtSecretEpilogue", Achievements.ending_bigdaddyhurtSecretEpilogue },
            { "saveEdited", Achievements.ending_saveedited },
        };

        public static void Main()
        {
            var app = new Program();
            Program.Run();
        }

        public static void Run()
        {

            Program program = new();

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

            // Set program to default language.
            CultureInfo.CurrentCulture = new CultureInfo(defaultLanguage);
            CultureInfo.CurrentUICulture = new CultureInfo(defaultLanguage);

            // Force parsing and internal operations to use dot as decimal separator, without changing the language.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;


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

            while (true)
            {
                program.MainMenu(false);
            }
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
                $"(2) {Generic.settings}\n" +
                $"(3) {Generic.exit}"
                );
            string input = Console.ReadLine();
            Console.WriteLine();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine(Generic.invalidkey);
                return;
            }
            else if (input.ToLower() == "1" || input.ToLower() == "loadfile")
            {
                LoadFile();
                return;
            }
            else if (input.ToLower() == "2" || input.ToLower() == "settings")
            {
                OpenSettings();
                return;
            }
            else if (input.ToLower() == "3" || input.ToLower() == "exit")
            {
                Environment.Exit(0);
                return;
            }

            /*switch (key.Key)
            {
                case ConsoleKey.D1:
                    LoadFile();
                    break;
                case ConsoleKey.D2:
                    OpenSettings();
                    break;
                default:
                    Console.WriteLine(Generic.invalidkey);
                    return;
            }*/
        }

        public void LoadFile(bool clearConsole = true)
        {
            if (clearConsole)
            {
                Console.Clear();
            }
            Console.WriteLine($" -- {Generic.loadfile} --");
            Console.WriteLine();
            Console.WriteLine(Generic.loadfile_instructions);
            Console.WriteLine();
            Console.WriteLine(
                $"{Generic.commonpaths}\n" +
                $"(1) {Generic.documents}\n" +
                $"(2) {Generic.desktop}\n" +
                $"(3) {Generic.downloads}\n" +
                $"(4) {Generic.appdata} ({Generic.thisfilelocationisdiscouraged}.)"
                );

            string savefile = String.Empty;

            Console.WriteLine();
            string input = Console.ReadLine();
            Console.WriteLine();

            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string os = Environment.OSVersion.Platform.ToString().ToLower();

            // Normalize input
            input = input.ToLowerInvariant();

            if (input == "documents" || input == "1")
            {
                input = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else if (input == "desktop" || input == "2")
            {
                input = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else if (input == "downloads" || input == "3")
            {
                input = Path.Combine(home, "Downloads");
            }
            else if (input == "appdata" || input == "4")
            {
                if (os.Contains("win"))
                {
                    // Windows-specific path
                    var localLow = Path.Combine(home, "AppData", "LocalLow");
                    input = Path.Combine(localLow, "IncontinentCell", "My Dystopian Robot Girlfriend", "Saves");
                }
                else if (os.Contains("mac"))
                {
                    // macOS: typically in Library/Application Support
                    var lib = Path.Combine(home, "Library", "Application Support");
                    input = Path.Combine(lib, "IncontinentCell", "My Dystopian Robot Girlfriend", "Saves");
                }
                else
                {
                    // Linux: follow XDG spec or fallback to ~/.config
                    var config = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Path.Combine(home, ".config");
                    input = Path.Combine(config, "IncontinentCell", "My Dystopian Robot Girlfriend", "Saves");
                }
            }

            // Did the user input a file or a directory?
            if (File.Exists(input))
            {
                // Does the file end with .mdrg? If not, tell user to try again.
                if (!input.EndsWith(".mdrg"))
                {
                    Console.WriteLine(Generic.loadfile_invalid);
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
                    Console.WriteLine(
                        Generic.loadfile_nofilesfound +
                        $"\n({input})"
                        );
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
                    filePath = Path.GetFullPath(files[fileIndex - 1]);
                }
                else
                {
                    Console.WriteLine(Generic.invalidkey);
                    return;
                }
            }
            else
            {
                Console.WriteLine(Generic.loadfile_invalid);
                Console.WriteLine($"({input})");
                return;
            }

            // If we have a file, continue.
            if (filePath != null)
            {
                ProcessFile(filePath);
            }
        }

        /// <summary>
        /// Processes the file and creates lists of the values, then passes them to the SavefileMenu().
        /// </summary>
        /// <param name="filePath"></param>
        public void ProcessFile(string filePath, bool clearConsole = true)
        {
            if (clearConsole)
            {
                Console.Clear();
            }

            fileContent = File.ReadAllText(filePath);
            savefileJson = JObject.Parse(fileContent);

            // Select save file
            selectedSaveFile = SelectSaveFile(filePath, true);

            if (selectedSaveFile == -1) {
                // This certainly should not happen.
                Console.WriteLine(Generic.selectsaveslot_invalidsaveslot);
                return;
            }

            // Test that there is data
            JObject savedataObject = null;
            JObject achievementsObject = null;
            try
            {
                savefileRoot = JObject.Parse(savefileJson.ToString());
                savedataObject = JObject.Parse(savefileJson["saves"][selectedSaveFile]["savedata"].ToString());
                achievementsObject = JObject.Parse(savefileRoot["achievements"].ToString()); // Achievements
            }
            catch
            {
                // Something else happened, please contact me.
                Console.WriteLine(Generic.pleasecontactme);
            }

            // Check if the savedata is null
            if (savedataObject == null)
            {
                // No savedata found, show error and specifically detail to the user that no, they cannot load an empty save file, and that they have to export it from the game FIRST.
                Console.WriteLine(Generic.nosavedata);
                return;
            }

            // Prepare three lists
            List<StringValue> stringValues = [];
            List<IntValue> intValues = [];
            List<DoubleValue> doubleValues = [];

            // Now that we have the savedata, we can create a dictionary, following the schema of InternalID, ID, and Data Type
            // TODO - Add all Key.Item2 values to the IDs.resx file.
            Dictionary<(string, string), Type> dataBindings = new()
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
                { ( "timesLostOldMaid", "data_timeslostoldmaid"), typeof(int) },
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

            // Switch cultures because NewtonSoft can't handle it. Set to default.
            (CultureInfo, CultureInfo) previousCulture = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

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
                    // Ensure we force it to be a double
                    JToken token = savedataObject[entry.Key.Item1];

                    // Check if the token is null or not
                    if (token != null)
                    {
                        // Check if the token is a double, int, or string
                        if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                        {
                            doubleValues.Add(new DoubleValue
                            {
                                ID = entry.Key.Item2,
                                Value = token.ToObject<double>(), // Correctly parse as double
                                InternalID = entry.Key.Item1
                            });
                        }
                        else if (token.Type == JTokenType.String)
                        {
                            double parsed;
                            // Try to parse the string as a double
                            if (double.TryParse(token.Value<string>(), NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
                            {
                                // Replace these fucking commas with dots. THIS CAUSED SO MUCH PAIN.
                                parsed = double.Parse(token.Value<string>().Replace(',', '.'), CultureInfo.InvariantCulture);
                                doubleValues.Add(new DoubleValue
                                {
                                    ID = entry.Key.Item2,
                                    Value = parsed, // Correctly assign parsed double
                                    InternalID = entry.Key.Item1
                                });
                            }
                            else
                            {
                                Console.WriteLine($"ERROR: Failed to parse double from string: {token}");
                            }
                        }
                    }
                }
            }

            saveSlot = savefileJson["saves"][selectedSaveFile].Value<int>("slot");
            int ingameTime = savefileJson["saves"][selectedSaveFile].Value<int>("ingameTime");
            notes = savefileJson["saves"][selectedSaveFile].Value<string>("notes");

            JArray jsVisitedWebsites = (JArray)savefileJson["visitedWebsites"];
            JArray jsGottenAchievements = (JArray)achievementsObject["values"];
            string[] visitedWebsites = jsVisitedWebsites.ToObject<string[]>();
            gottenAchievements = jsGottenAchievements.ToObject<string[]>();

            boolValues.Add(new BoolValue
            {
                ID = "data_lightswitch",
                Value = savedataObject.Value<bool>("lightSwitchOn"),
                InternalID = "lightSwitchOn"
            });

            // Convert raw time into days, hours, and minutes
            const int MPD = 24 * 60, MPH = 60;
            int streamTime = int.Parse(savedataObject.Value<int>("streamedFor").ToString());
            int longestStream = int.Parse(savedataObject.Value<int>("longestStream").ToString());

            void SplitTime(int totalMinutes, out int days, out int hours, out int minutes)
            {
                days = totalMinutes / MPD;
                int remainder = totalMinutes % MPD;
                hours = remainder / MPH;
                minutes = remainder % MPH;
            }

            SplitTime(streamTime, out streamDays, out streamHours, out streamMinutes);
            SplitTime(longestStream, out longestStreamDays, out longestStreamHours, out longestStreamMinutes);
            SplitTime(ingameTime, out ingameTimeDays, out ingameTimeHours, out ingameTimeMinutes);

            // Since we finished data processing, switch back to the user's culture.
            CultureInfo.CurrentCulture = previousCulture.Item1;
            CultureInfo.CurrentUICulture = previousCulture.Item2;

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

            weekdayText = rentDaysMapping.TryGetValue(savedataObject["statusText"].ToString(), out var day)
                    ? day
                    : Generic.WeekDaysUnknown;

            /// DO NOT ASSIGN MORE DATA HERE
            /// ASSIGN MORE DATA ABOVE THE
            /// CULTURE SWITCHING PART ABOVE

            // Data finished assigning.

            gottenAchievements = achievementsObject["values"].ToObject<string[]>() ?? [];
            visitedWebsites = savefileRoot["visitedWebsites"].ToObject<string[]>() ?? [];

            SavefileMenu(stringValues, intValues, doubleValues, boolValues, gottenAchievements, visitedWebsites, filePath, false);
        }

        public void SetNote(string note)
        {
            // Ask user to set a note.
            Console.WriteLine($"{Generic.input}:");
            notes = Console.ReadLine();
            return;
        }

        /// <summary>
        /// Displays the save file menu.
        /// </summary>
        /// <param name="stringValues"></param>
        /// <param name="intValues"></param>
        /// <param name="doubleValues"></param>
        /// <param name="clearConsole"></param>
        public void SavefileMenu(
            List<StringValue> stringValues,
            List<IntValue> intValues,
            List<DoubleValue> doubleValues,
            List<BoolValue> boolValues,
            string[] achievements,
            string[] visitedWebsites,
            string filePath,
            bool clearConsole = true
            )
        {
            if (clearConsole)
            {
                Console.Clear();
            }

            // Combine all values into a single list of objects
            List<object> allValues =
            [
                .. stringValues,
                .. intValues,
                .. doubleValues,
                .. boolValues,
            ];

            while (true)
            {
                Console.WriteLine($" -- {Generic.savefilemenu} --");
                Console.WriteLine();

                // Display data with numbers
                int i = 1;
                foreach (var item in allValues)
                {
                    if (item is StringValue stringValue)
                    {
                        Console.WriteLine($"({i:D3}) | (str) {stringValue.GetName()}: {stringValue.Value}");
                    }
                    else if (item is IntValue intValue)
                    {
                        Console.WriteLine($"({i:D3}) | (int) {intValue.GetName()}: {intValue.Value}");
                    }
                    else if (item is DoubleValue doubleValue)
                    {
                        // Format the double value explicitly to ensure proper decimal point display
                        string formattedValue = doubleValue.Value.ToString("R", CultureInfo.InvariantCulture);
                        Console.WriteLine($"({i:D3}) | (dbl) {doubleValue.GetName()}: {formattedValue}");
                    }
                    else if (item is BoolValue boolValue)
                    {
                        Console.WriteLine($"({i:D3}) | (bol) {boolValue.GetName()}: {boolValue.Value}");
                    }
                    i++;
                }

                Console.WriteLine($"({i:D3}) | (str) {IDs.data_notes}: {notes}");

                // Specially formatted values, like streamed time, longest stream, ingame time, and rent.
                Console.WriteLine($"({i:D3}) | (int) {IDs.data_streamedfor}: {streamDays}d {streamHours}h {streamMinutes}m");
                i++;
                Console.WriteLine($"({i:D3}) | (int) {IDs.data_longeststream}: {longestStreamDays}d {longestStreamHours}h {longestStreamMinutes}m");
                i++;
                Console.WriteLine($"({i:D3}) | (int) {IDs.data_ingametime}: {ingameTimeDays}d {ingameTimeHours}h {ingameTimeMinutes}m");
                i++;
                Console.WriteLine($"({i:D3}) | (int) {IDs.data_rent}: {weekdayText}");
                i++;
                Console.WriteLine($"({i:D3}) | (int) {Generic.slot}: {saveSlot}");

                /// Save editing disabled until MDRG dev switches away from his horrible JSON format.
                Console.WriteLine();
                Console.WriteLine($"(A) {Generic.toachievements}");
                //Console.WriteLine($"(N) {Generic.setnote} ({notes})");
                Console.WriteLine($"(E) {Generic.exit}");
                Console.WriteLine($"(B) {Generic.createbackup}");
                //Console.WriteLine($"(S) {Generic.save}");

                Console.WriteLine();
                // Ask user to select a value to edit, or type "e" to exit and return to the main menu.
                Console.WriteLine($"{Generic.input}:");
                string input = Console.ReadLine();
                Console.WriteLine();

                /*
                // Check if the input is a number and within the range of values
                if (int.TryParse(input, out int selectedValue) && selectedValue > 0 && selectedValue <= allValues.Count)
                {
                    // -1 because we need to account for the zero-based index
                    var selectedItem = allValues[selectedValue - 1];
                    // Check the type of the selected item and allow editing
                    if (selectedItem is StringValue stringValue)
                    {
                        Console.WriteLine($"{Generic.currentlyediting} {stringValue.GetName()} (String): {stringValue.Value}");
                        Console.WriteLine($"{Generic.input}:");
                        string newValue = Console.ReadLine();
                        // Check if the new value is null or empty

                        if (string.IsNullOrEmpty(newValue))
                        {
                            Console.WriteLine(Generic.invalidkey);
                            return;
                        }

                        stringValue.Value = newValue;
                    }
                    else if (selectedItem is IntValue intValue)
                    {
                        Console.WriteLine($"{Generic.currentlyediting} {intValue.GetName()} (Int): {intValue.Value}");
                        Console.WriteLine($"{Generic.input}:");
                        string newValue = Console.ReadLine();
                        // Check if the new value is null or empty

                        if (string.IsNullOrEmpty(newValue))
                        {
                            Console.WriteLine(Generic.invalidkey);
                            return;
                        }

                        intValue.FromString(newValue);
                    }
                    else if (selectedItem is DoubleValue doubleValue)
                    {
                        Console.WriteLine($"{Generic.currentlyediting} {doubleValue.GetName()} (Double): {doubleValue.Value}");
                        Console.WriteLine($"{Generic.input}:");
                        string newValue = Console.ReadLine();

                        // Check if the new value is null or empty
                        if (string.IsNullOrEmpty(newValue))
                        {
                            Console.WriteLine(Generic.invalidkey);
                            return;
                        }

                        // Try to parse the new value as a double
                        if (double.TryParse(newValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedValue))
                        {
                            doubleValue.Value = parsedValue;
                        }
                        else
                        {
                            Console.WriteLine(Generic.invalidkey);
                        }
                    }
                    else if (selectedItem is BoolValue boolValue)
                    {
                        Console.WriteLine($"{Generic.currentlyediting} {boolValue.GetName()} (Bool): {boolValue.Value}");
                        Console.WriteLine($"{Generic.input}:");
                        string newValue = Console.ReadLine();
                        // Check if the new value is null or empty
                        if (string.IsNullOrEmpty(newValue))
                        {
                            Console.WriteLine(Generic.invalidkey);
                            return;
                        }
                        // Try to parse the new value as a boolean
                        if (bool.TryParse(newValue, out bool parsedValue))
                        {
                            boolValue.Value = parsedValue;
                        }
                        else
                        {
                            Console.WriteLine(Generic.invalidkey);
                        }
                    }
                    SavefileMenu(stringValues, intValues, doubleValues, boolValues, achievements, visitedWebsites, filePath, false);
                }

                else */
                if (input.ToLower() == "e")
                {
                    return;
                }
                /*
                else if (input.ToLower() == "n")
                {
                    SetNote(notes);
                    return;
                }
                */
                else if (input.ToLower() == "b")
                {
                    CreateBackup(filePath);
                    createdBackup = true;
                    return;
                }
                else if (input.ToLower() == "a")
                {
                    AchievementsMenu();
                }
                /*
                else if (input.ToLower() == "s")
                {
                    // Because when we continue from here, the variable is emptied, we *have* to pass it to the Save() method.

                    Save(stringValues, intValues, doubleValues, boolValues, filePath, true);
                }
                */
                else
                {
                    Console.Clear();
                    Console.WriteLine(Generic.invalidkey);
                }
            }
        }

        /*
        public void Save(
            List<StringValue> stringValues,
            List<IntValue> intValues,
            List<DoubleValue> doubleValues,
            List<BoolValue> boolValues,
            string filePath,
            bool clearConsole = true
            )
        {
            if (clearConsole)
            {
                Console.Clear();
            }

            Console.WriteLine($" -- {Generic.saving} --");
            Console.WriteLine();

            // Prepare JSON for saving
            Console.WriteLine($"{Generic.preparing_json}");
            JObject savedataObject = JObject.Parse(savefileJson["saves"][selectedSaveFile]["savedata"].ToString());
            Console.Write(".");
            JArray jsGottenAchievements = [];
            if ((JArray)achievementsObject["values"] is null)
            {
                jsGottenAchievements = [];
            }
            else
            {
                jsGottenAchievements = (JArray)achievementsObject["values"];
            }
            Console.Write(".");
            JArray jsVisitedWebsites = [];
            if ((JArray)savefileJson["visitedWebsites"] is null)
            {
                jsVisitedWebsites = [];
            }
            else
            {
                jsVisitedWebsites = (JArray)savefileJson["visitedWebsites"];
            }
            Console.Write(".");
            string[] visitedWebsites = jsVisitedWebsites.ToObject<string[]>();
            Console.Write(".");
            string[] gottenAchievements = jsGottenAchievements.ToObject<string[]>();
            Console.Write(".");
            List<string> achievementsSelected = [];
            Console.Write($" {Generic.done}!");

            // Now we use a for loop to iterate through all values and set them to the new value.
            Console.WriteLine($"{Generic.combining_json}");
            foreach (var item in stringValues)
            {
                savedataObject[item.InternalID] = item.Value;
                Console.WriteLine($"DEBUG - Setting {item.InternalID} to {item.Value}");
            }
            Console.Write(".");
            foreach (var item in intValues)
            {
                savedataObject[item.InternalID] = item.Value;
                Console.WriteLine($"DEBUG - Setting {item.InternalID} to {item.Value}");
            }
            Console.Write(".");
            foreach (var item in doubleValues)
            {
                savedataObject[item.InternalID] = item.Value;
                Console.WriteLine($"DEBUG - Setting {item.InternalID} to {item.Value}");
            }
            Console.Write(".");
            foreach (var item in boolValues)
            {
                savedataObject[item.InternalID] = item.Value;
                Console.WriteLine($"DEBUG - Setting {item.InternalID} to {item.Value}");
            }
            Console.Write($" {Generic.done}");

            // Now we need to set the achievements and websites
            Console.WriteLine($"{Generic.updating_achievements}");
            // Check if the achievements are null. If they are, we don't need to do anything.
            if (achievementsObject != null)
            {
                // Set the achievements to the new value
                achievementsObject["values"] = JArray.FromObject(gottenAchievements);
            }
            Console.Write(".");
            Console.Write($" {Generic.done}");

            // Now we need to set the websites
            Console.WriteLine($"{Generic.updating_websites}");
            // Check if the websites are null. If they are, we don't need to do anything.
            if (jsVisitedWebsites != null)
            {
                // Replace the entire array with the new value
                jsVisitedWebsites = JArray.FromObject(visitedWebsites);
            }
            Console.Write(".");
            Console.Write($" {Generic.done}");

            // Now, to save the file, we need to set the new values to the savefileRoot
            Console.WriteLine($"{Generic.preparing_json}");
            savefileJson["saves"][selectedSaveFile]["savedata"] = savedataObject;
            Console.Write(".");
            savefileJson["achievements"] = achievementsObject;
            Console.Write(".");
            savefileJson["visitedWebsites"] = jsVisitedWebsites;
            Console.Write($" {Generic.done}");

            // Now we need to save the file
            Console.WriteLine($"{Generic.saving_file}");
            // Check if the file exists. If it does not, we make it.
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
            Console.Write(".");
            // Save to the file
            File.WriteAllText(filePath, savefileJson.ToString());
            Console.Write($" {Generic.done}");

            // Quickly get the full path to show the user where on their computer the file is.
            string fileName = Path.GetFileName(filePath);
            string directoryPath = Path.GetDirectoryName(filePath);
            string fileSavePath = Path.Combine(directoryPath, fileName);
            Console.WriteLine($"{Generic.filesavedat}: {fileSavePath}");
        }
        */

        /// <summary>
        /// Displays the achievements menu, allowing the user to unlock or lock achievements.
        /// </summary>
        /// <param name="clearConsole"></param>
        public void AchievementsMenu(bool clearConsole = true)
        {
            if (clearConsole)
            {
                Console.Clear();
            }

            while (true)
            {
                Console.WriteLine($" -- {Generic.achievements} --");
                // Since we have the achievements already, and we have a list of all achievements, we can display whether the User has them via "string - localized string - bool" format and a corresponding number.

                foreach (var achievement in allAchievements)
                {
                    // Check if the user has the achievement
                    bool hasAchievement = gottenAchievements.Contains(achievement.Key);
                    string status = hasAchievement ? Generic.achievements_obtained : Generic.achievements_unobtained;
                    Console.WriteLine($"({Array.IndexOf(allAchievements.Keys.ToArray(), achievement.Key) + 1}) {achievement.Value} - {status}");
                }

                Console.WriteLine();
                // Now that we have the achievements, we can ask the user to select one to unlock or lock.
                Console.WriteLine($"(E) {Generic.exit}");
                Console.WriteLine($"{Generic.input}:");
                string input = Console.ReadLine();
                /*
                // Check if the input is a number and within the range of achievements
                if (int.TryParse(input, out int selectedAchievement) && selectedAchievement > 0 && selectedAchievement <= allAchievements.Count)
                {
                    // Since the achievements work by either having it there or it not being in the ending JSON, we can just add or remove it from the list.
                    var selectedKey = allAchievements.Keys.ToArray()[selectedAchievement - 1];
                    if (gottenAchievements.Contains(selectedKey))
                    {
                        // Remove the achievement
                        gottenAchievements = gottenAchievements.Where(x => x != selectedKey).ToArray();
                        Console.WriteLine($"{Generic.achievements_removed} {selectedKey}");
                    }
                    else
                    {
                        // Add the achievement
                        gottenAchievements = gottenAchievements.Append(selectedKey).ToArray();
                        Console.WriteLine($"{Generic.achievements_added} {selectedKey}");
                    }
                    AchievementsMenu(true);
                }
                else */
                if (input.ToLower() == "e")
                {
                    return;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine(Generic.invalidkey + "\n");
                }
            }
        }

        /// <summary>
        /// Creates a backup of the save file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void CreateBackup(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath", "File path cannot be null.");
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

                Console.Clear();

                Console.WriteLine($"{Generic.backupcreated}: {newBackupFileName}");
            }
        }

        /// <summary>
        /// Select a save file from the list of saves in the JSON file (where filePath points towards).
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public int SelectSaveFile(string filePath, bool clearConsole = true)
        {
            if (clearConsole)
            {
                Console.Clear();
            }
            // Check how many non-autosaves there are
            // Load save file into variable, then parse the JSon
            string fileContent = File.ReadAllText(filePath);
            savefileJson = JObject.Parse(fileContent);

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
                return -1;
            }
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
                settings = new Dictionary<string, string>
                {
                    // If DefaultLanguage is null, create it.
                    ["DefaultLanguage"] = "en-US"
                };
                // Save settings back to JSON file
                json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText("Settings.json", json);
            }


            Console.WriteLine(
                $"\n{Generic.settings_instructions}\n\n" +
                $"(1) {Generic.settings_defaultlanguage}: {new CultureInfo(settings["DefaultLanguage"]).DisplayName}\n" +
                $"\n" +
                $"(E) {Generic.exit}"
            );
            string input = Console.ReadLine();
            Console.Clear();
            switch (input)
            {
                // Default Language
                case "1":
                    Console.WriteLine($" -- {Generic.settings_defaultlanguage} --");
                    Console.WriteLine();
                    Console.WriteLine(
                        $"{Generic.settings_defaultlanguage_prompt}\n\n" +
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
                    Console.WriteLine();
                    Console.WriteLine($"{Generic.input}:");
                    string cultureKey = Console.ReadLine();
                    Console.WriteLine();
                    // Choose language based on input
                    if (cultureKey.Equals("1", StringComparison.InvariantCultureIgnoreCase))
                    {
                        settings["DefaultLanguage"] = "en-US";
                        CultureInfo.CurrentCulture = new CultureInfo("en-US");
                        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
                    }
                    // Check if the input is a number and within the range of available cultures
                    else if (int.TryParse(cultureKey, out int cultureIndex) && cultureIndex > 1 && cultureIndex <= availableCultures.Count + 1)
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
                case "e":
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

    /// <summary>
    /// Class representing a value with a double. Not to be used for values expecting any string.
    /// </summary>
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

    /// <summary>
    /// Class representing a boolean value. Not to be used for values that should retain any other type.
    /// </summary>
    public class BoolValue
    {
        /// <summary>
        /// Localization key. Must be unique and exist in the `IDs.resx` file.
        /// </summary>
        public required string ID { get; set; }

        /// <summary>
        /// The actual value used in the program. Typically deserialized from JSON.
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// The unique identifier used to locate this value in external JSON.
        /// </summary>
        public required string InternalID { get; set; }

        /// <summary>
        /// Gets the localized name for this value using its ID.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            LocalizationHelper locHelper = new();
            return locHelper.GetLocalizedName(ID);
        }
    }
}