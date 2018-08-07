using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using FluentScheduler;

// Need:
// Logging
// Reading events & token from file

//https://discord.foxbot.me/docs/api/Discord.Webhook.DiscordWebhookClient.html
//https://github.com/fluentscheduler/FluentScheduler

namespace Spudule
{
    class Program
    {
        private static DiscordWebhookClient hook;
        private static Timer timer;

        static void Main(string[] args)
        {
            hook = new DiscordWebhookClient();
            var embed = new EmbedBuilder()
                .WithTitle("Test")
                .WithImageUrl("https://pbs.twimg.com/profile_images/559119938687107072/i6Ox_iOk_400x400.jpeg").Build();

            var registry = new Registry();
            registry.Schedule(() =>
            {
                hook.SendMessageAsync("timer eevent", false, null, "Spudule", "http://www.dutchdc.com/wp-content/uploads/2016/12/Potato_shadow.png");
            }).ToRunNow().AndEvery(1).Minutes();

            JobManager.Initialize(registry);

            Console.ReadLine();
            //while (true) {
            //    hook.SendMessageAsync("test test", false, new Embed[] { embed }, "Spudule", "http://www.dutchdc.com/wp-content/uploads/2016/12/Potato_shadow.png");
                
            //    Console.ReadLine();
            //}

        }

        private static Task TimerEvent()
        {
            Console.WriteLine("Fired");
            hook.SendMessageAsync("timer eevent", false, null, "Spudule", "http://www.dutchdc.com/wp-content/uploads/2016/12/Potato_shadow.png");

            return Task.CompletedTask;
        }
    }
}
