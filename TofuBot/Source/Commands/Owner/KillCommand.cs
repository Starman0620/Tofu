using System;
using System.Threading.Tasks;

using Discord.Commands;

using TofuBot.Utils;

namespace TofuBot.Commands
{
    public class KillCommand : ModuleBase<SocketCommandContext>
    {
        [Command("kill")]
        [Summary("Safely kill the bot|")]
        [Priority(Category.Owner)]
        public async Task Kill()
        {
			if(Context.Message.Author.Id != Bot.config.OwnerID) return;
			await ReplyAsync("Shutting down...");
            DailyReportSystem.CreateBackup();
			Log.Write("Shutdown triggered by command.");
			Environment.Exit(0);
        }
    }
}