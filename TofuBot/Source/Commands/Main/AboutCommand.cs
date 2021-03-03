using System;
using System.IO;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Humanizer;

namespace TofuBot.Commands
{
    public class AboutCommand : ModuleBase<SocketCommandContext>
    {
        [Command("about")]
        [Summary("Shows information about the bot|")]
        [Priority(Category.Main)]
        public async Task About(string arg = null)
        {
            var uptime = DateTime.Now.Subtract(Bot.startTime);

            EmbedBuilder eb = new EmbedBuilder();
            eb.WithColor(Bot.config.embedColour);
            eb.WithTitle("Tofu");
            eb.WithThumbnailUrl(Bot.client.CurrentUser.GetAvatarUrl());
            eb.AddField("Language", "C#", true);
            eb.AddField("Library", "Discord.NET 2.3.0", true);
            eb.AddField("Author", "Starman0620#8456", true);
            eb.AddField("Member Count", Context.Guild.MemberCount, true);
            eb.AddField("Uptime", uptime.Humanize(3), true);

            await ReplyAsync("", false, eb.Build());
        }
    }
}