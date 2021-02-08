using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Discord;
using Discord.WebSocket;

namespace TofuBot.Utils
{
    public class DailyReportSystem
    {
        public static DailyReport Report = new DailyReport()
        {
            DayOfReport = DateTime.Now,
            CommandsRan = 0,
            MessagesSent = 0,
            UsersJoined = 0,
            UsersLeft = 0
        };

        public static Task Init()
        {
			// Load the temp report if it exists. This whole temp report nonsense is just to allow the bot to restart without losing progress
			if(File.Exists("DailyReports/tempReport.json"))
				Report = JsonConvert.DeserializeObject<DailyReport>(File.ReadAllText("DailyReports/tempReport.json"));

			// E v e n t s
			// (Used for actually keeping track of things)
            Bot.client.MessageReceived += (SocketMessage Message) =>
            {
                Report.MessagesSent++;
                if (Message.Content.StartsWith(".")) // Inaccurate but oh well.
                    Report.CommandsRan++;
                return Task.CompletedTask;
            };
            Bot.client.UserJoined += (SocketGuildUser User) =>
            {
                Report.UsersJoined++;
                return Task.CompletedTask;
            };
            Bot.client.UserLeft += (SocketGuildUser User) =>
            {
                Report.UsersLeft++;
                return Task.CompletedTask;
            };

			// Main report timer
            Timer t = new Timer(30000); // 1 sec = 1000, 60 sec = 60000, this is set to 30 seconds
            t.AutoReset = true;
            t.Elapsed += Elapsed;
            t.Start();

			// Backup timer
            Timer t2 = new Timer(300000); // 5 minutes
            t2.AutoReset = true;
            t2.Elapsed += Backup;
            t2.Start();

            return Task.CompletedTask;
        }

        private static void Elapsed(object sender, ElapsedEventArgs e)
        {
			// If it is a new day, send the report and reset
            if (DateTime.Now.Day != Report.DayOfReport.Day) SendReport(true);
        }

        private async static void SendReport(bool IsDaily)
        {
            SocketGuild Guild = Bot.client.Guilds.FirstOrDefault();
            SocketTextChannel LogChannel = (SocketTextChannel)Guild.GetChannel(Globals.BotLog);

            try
            {
				// Ensure the report directory exists
                if (!Directory.Exists("DailyReports"))
                    Directory.CreateDirectory("DailyReports");

				// Save the report for the current day
                string json = JsonConvert.SerializeObject(Report, Formatting.Indented);
                File.WriteAllText("DailyReports/" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + " Report.json", json);

				// Create and send the report embed to the bot log channel
                var eb = new EmbedBuilder();
                eb.WithTitle("Daily Report For " + Report.DayOfReport.ToString("dddd, dd MMMM yyyy"));
                eb.WithTimestamp(Report.DayOfReport);
                eb.AddField("Messages Sent", Report.MessagesSent, true);
                eb.AddField("Commands Ran", Report.CommandsRan, true);
                eb.AddField("Users Joined", Report.UsersJoined, true);
                eb.AddField("Users Left", Report.UsersLeft, true);
                eb.WithColor(Bot.config.embedColour);
                await LogChannel.SendMessageAsync("", false, eb.Build());

				// Reset the report, if this is a daily report (this whole thing is left over from one of my previous bots where reports could be manually triggered)
                if (IsDaily)
                {
                    Report = new DailyReport()
                    {
                        DayOfReport = DateTime.Now,
                        CommandsRan = 0,
                        MessagesSent = 0,
                        UsersJoined = 0,
                        UsersLeft = 0
                    };
                }

				// Remove the temp report so it doesn't load itself in the next day
                File.Delete("DailyReports/tempReport.json");
            }
            catch (Exception ex)
            {
                await LogChannel.SendMessageAsync($"Failed to send report!\n{ex.Message}\n{ex.TargetSite}\n{ex.Source}\n{ex.StackTrace}\n{ex.InnerException}");
            }
        }

        private static void Backup(object sender, ElapsedEventArgs e)
        {
			// Save the report to a temporary file
            string Json = JsonConvert.SerializeObject(Report, Formatting.Indented);
            File.WriteAllText("DailyReports/tempReport.json", Json);
        }
    }

    public class DailyReport
    {
        public DateTime DayOfReport { get; set; }
        public int MessagesSent { get; set; }
        public int CommandsRan { get; set; }
        public int UsersJoined { get; set; }
        public int UsersLeft { get; set; }
    }
}