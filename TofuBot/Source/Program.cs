using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Newtonsoft.Json;

using TofuBot.Utils;
using TofuBot.Commands;

namespace TofuBot
{
    class Bot
    {
        static void Main(string[] args) => new Bot().RunBot().GetAwaiter().GetResult(); // Point the main function to the async RunBot task so the bot can operate asynchronously

        public static DiscordSocketClient client = new DiscordSocketClient();
        public static CommandService commands = new CommandService();
        public static IServiceProvider services;
        public static BotConfig config;
        public static DateTime startTime = DateTime.Now;

        public async Task RunBot()
        {
#if DEBUG
            if (!Directory.Exists("../WorkingDir")) Directory.CreateDirectory("../WorkingDir");
            Directory.SetCurrentDirectory("../WorkingDir");
#endif

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .BuildServiceProvider();
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            // If no config file is present, create a new template one and quit
            if (!File.Exists("config.json"))
            {
                config = new BotConfig()
                {
                    Prefix = "~",
                    Token = "",
                    Status = ""
                };
                File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
                Log.Write("No configuration file present!");
                Log.Write("A template configuration file has been written to config.json");
                Environment.Exit(0);
            }
            else // If there is a config, read it
                config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.json"));

            // Set up events
            client.Log += (LogMessage message) =>
            {
                Log.Write(message.Message);
                return Task.CompletedTask;
            };
            client.MessageReceived += HandleCommandAsync;
			client.UserJoined += async (SocketGuildUser user) => {
				await ((SocketTextChannel)client.GetChannel(config.WelcomeChannel)).SendMessageAsync($"Welcome, {user.Mention} to Cerro Gordo! Be sure to read the <#774567486069800960> before chatting!");
			};


            // Start the bot
            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();
            await client.SetGameAsync(config.Status);
			await DailyReportSystem.Init();
			await Trivia.Init();

            await Task.Delay(-1); // Wait forever
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Basic setup for handling the command
            string messageLower = arg.Content.ToLower();
            SocketUserMessage message = arg as SocketUserMessage;
            if (message == null || message.Author.IsBot && !message.Author.IsWebhook) return;
            int argumentPos = 0; // The location where the prefix should be found

            if (message.HasStringPrefix(config.Prefix, ref argumentPos) || message.HasMentionPrefix(client.CurrentUser, ref argumentPos))
            { 
                SocketCommandContext context = new SocketCommandContext(client, message); // Create context for the command, this is things like channel, guild, etc
                var result = await commands.ExecuteAsync(context, argumentPos, services); // Execute the command with the above context
				Log.Write($"{arg.Author} executed command: {arg.Content}", false);

                // Command error handling
                if (!result.IsSuccess && !result.ErrorReason.ToLower().Contains("unknown command"))
				{
					Log.Write(result.ErrorReason);

					// Get command usage
					string command = arg.Content.ToLower().Split(" ")[0].Replace(config.Prefix, "");
					string usage = TofuBot.Commands.Main.HelpCommand.GetCommandUsage(command);

					// Send a help embed
					EmbedBuilder helpEmbed = new EmbedBuilder();
					helpEmbed.WithColor(config.embedColour);
					string upperCommandName = command[0].ToString().ToUpper() + command.Remove(0, 1);
					helpEmbed.WithTitle($"{upperCommandName} Command");
					helpEmbed.WithDescription($"{usage}");
					await message.Channel.SendMessageAsync("There was an error executing your command! Are you sure you've used it correctly?", false, helpEmbed.Build());

					//await message.Channel.SendMessageAsync($"⚠️ Error: {result.ErrorReason} ⚠️\nConsult Starman or the help page for the command you executed. (.help [command])");
				}
				else if (result.IsSuccess)
					Log.Write($"{arg.Author} executed command: {arg.Content}", false);
            }
        }
	}

    public class BotConfig
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string Status { get; set; }
		public ulong OwnerID { get; set; }
		public ulong WelcomeChannel { get; set; }
		public ulong LogChannel { get; set; } = 1;
        public string CatAPIKey { get; set; } // Not used currently in tofu, left in just in case
        public string WikiHowAPIKey { get; set; } // rapidapi.com WikiHow; not used in tofu. Likely never will be
        public string WeatherAPIKey { get; set; }
        public Color embedColour { get; set; } = Color.LighterGrey;
    }
}