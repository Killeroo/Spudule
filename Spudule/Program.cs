using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Discord;
using Discord.Webhook;
using FluentScheduler;
using System.Linq;

// Need:
// Logging
// Reading events & token from file

//https://discord.foxbot.me/docs/api/Discord.Webhook.DiscordWebhookClient.html
//https://github.com/fluentscheduler/FluentScheduler

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

        static void Main(string[] args)
        {
            // Try read config file and load values
            events = ReadConfigFile("config.txt");

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
                var num = MentionUtils.ParseUser("@" + e.Item2);
                // TODO: Trying to get user mention
                embeds.Add(new EmbedBuilder()
                .WithTitle($"{emoji} Reminder")
                .WithDescription($" {e.Item3}").Build());
            }

            // Setup hook
            hook = new DiscordWebhookClient(id, token);
            logger.Info("Discord Webhook created");

            //TODO: Comment more of this 
            var registry = new Registry();
            registry.Schedule(() =>
            {
                hook.SendMessageAsync("", false, new Embed[] { embeds[0] }, "Spudule", "http://www.dutchdc.com/wp-content/uploads/2016/12/Potato_shadow.png");
            }).ToRunNow().AndEvery(1).Minutes();

            JobManager.Initialize(registry);

            //hook.SendMessageAsync("test test", false, new Embed[] { embed }, "Spudule", "http://www.dutchdc.com/wp-content/uploads/2016/12/Potato_shadow.png");

            Console.ReadLine();
            //while (true) {
            //    hook.SendMessageAsync("test test", false, new Embed[] { embed }, "Spudule", "http://www.dutchdc.com/wp-content/uploads/2016/12/Potato_shadow.png");
                
            //    Console.ReadLine();
            //}

        }

        private static List<Tuple<string, string, string>> ReadConfigFile(string path)
        {
            List<Tuple<string, string, string>> events = new List<Tuple<string, string, string>>();

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
