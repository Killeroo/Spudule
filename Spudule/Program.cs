using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using Discord;
using Discord.Webhook;

using FluentScheduler;

namespace Spudule
{
    class Program
    {
        // Holds event data in following format (time eg 16.00), (user), (message)
        private static List<Tuple<string, string, string>> events;

        private static DiscordWebhookClient hook;
        private static List<Embed> embeds = new List<Embed>();
        private static Logger logger = new Logger();
        private static ulong id = 0;
        private static string token;
        private static string configPath = "config.txt";

        static void Main(string[] args)
        {
            // Load config path from argument if there are any
            if (args.Length > 0) {
                configPath = args[0];
            }

            // Try read config file and load values
            events = ReadConfigFile(configPath);

            // Sanity check some values
            if (String.IsNullOrEmpty(token) || id == 0 || events.Count == 0) {
                logger.Critical("Problem reading config file, cannot find an event, token or id");
                Console.WriteLine("config.txt should be formatted with the following:\n" +
                    "\n\ttoken asd23emk2ek12nmk2l1mn2\n" +
                    "\tid 21312535523532\n" +
                    "\tevent 16:00 killeroo \"meow meow meow\"\n");
            }

            // Setup embeds for each event
            var emoji = new Emoji(":loudspeaker:");
            foreach (var e in events) {
                // Try get user name (TODO: Fix mentions)
                ulong unum = 0;
                string descText = "";
                if (MentionUtils.TryParseUser(e.Item2, out unum)) {
                    descText = MentionUtils.MentionUser(unum) + " - " + Format.Italics(e.Item3.Replace("\"", string.Empty));
                } else {
                    descText = Format.Bold(e.Item2) + " - " + Format.Italics(e.Item3.Replace("\"", string.Empty));
                }

                // Construct embed
                embeds.Add(new EmbedBuilder()
                .WithTitle($"{emoji} Reminder")
                .WithDescription(descText)
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(Color.DarkOrange).Build());
            }

            // Setup hook
            hook = new DiscordWebhookClient(id, token);
            logger.Info("Discord Webhook created");

            // Schedule events
            var registry = new Registry();
            for (int i = 0; i < events.Count; i++) {

                var e = embeds[i];
                var t = events[i];
                var time = events[i].Item1.Split(':');

                // (Hacky)
                if (time.Length == 3) { // If there is a day in the time code 
                    // Try convert to dayofweek enum
                    Enum.TryParse(time[0], out DayOfWeek day);

                    registry.Schedule(() => { // Add event
                        hook.SendMessageAsync("", false, new Embed[] { e }, "Spudule", "http://www.dutchdc.com/wp-content/uploads/2016/12/Potato_shadow.png");
                        logger.Info($"Event fired: {t.Item1} {t.Item2} {t.Item3}");

                    }).ToRunEvery(1).Weeks().On(day).At(Convert.ToInt32(time[1]), Convert.ToInt32(time[2]));
                } else {
                    registry.Schedule(() => { // Add event
                        hook.SendMessageAsync("", false, new Embed[] { e }, "Spudule", "http://www.dutchdc.com/wp-content/uploads/2016/12/Potato_shadow.png");
                        logger.Info($"Event fired: {t.Item1} {t.Item2} {t.Item3}");

                    }).ToRunEvery(1).Days().At(Convert.ToInt32(events[i].Item1.Split(':')[0]), Convert.ToInt32(events[i].Item1.Split(':')[1]));
                }

                logger.Debug($"Added event: {events[i].Item1} {events[i].Item2} {events[i].Item3}");
            }

            JobManager.Initialize(registry);
            logger.Info("Events scheduled");

            logger.Info("Scheduler running. Press Control-C to quit... ");

            // Hack to keep the program running
            while (true)
                Thread.Sleep(1000);

        }

        private static List<Tuple<string, string, string>> ReadConfigFile(string path)
        {
            List<Tuple<string, string, string>> events = new List<Tuple<string, string, string>>();
            logger.Info($"Reading config file @ {configPath}");

            try {
                // Open config file (create if not found)
                var lines = File.ReadAllLines(path);
                foreach (var line in lines) {
                    var words = line.Split(' ');

                    // Process based on the first word of each line
                    switch (words[0].ToLower()) {
                        case "event": // Format example: event 16:00 killeroo "something here"
                            var regex = new Regex("\"[^\"]*\"");
                            var matches = regex.Matches(line).Cast<Match>().Select(m => m.Value).ToArray(); // Find strings inbetween quotation marks and save as message
                            events.Add(new Tuple<string, string, string>(words[1], words[2], matches[0]));
                            logger.Debug($"Added event: time={words[1]} username={words[2]} message={matches[0]}");
                            break;
                        case "token": // Format example: token kn21k32knk2n32kn
                            token = words[1];
                            logger.Debug($"Token: {words[1]}");
                            break;
                        case "id": // Format example: id 2312315322
                            // Convert without loss of precision
                            decimal d = Decimal.Parse(words[1]);
                            ulong u = (ulong)d;
                            id = u;
                            logger.Debug($"ID detected: {words[1]}");
                            break;
                        default:
                            break;
                    }
                }

            } catch (Exception e) {
                logger.Critical($"type={e.GetType().ToString()} msg=\"{e.Message}\"");
                Environment.Exit(1);
            }

            logger.Info("Config file read");
            return events;
        }

        private static Task TimerEvent()
        {
            Console.WriteLine("Fired");
            hook.SendMessageAsync("timer eevent", false, null, "Spudule", "http://www.dutchdc.com/wp-content/uploads/2016/12/Potato_shadow.png");

            return Task.CompletedTask;
        }
    }
}
