using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using ScottPlot;

using Newtonsoft.Json;

using TofuBot.Utils;

namespace TofuBot.Commands
{
	public class ServerStatsCommand : ModuleBase<SocketCommandContext>
	{
		[Command("serverstats")]
		[Priority(Category.Staff)]
		[Summary("Show basic statistics about the server|")]
		public async Task ServerStats([Remainder]string args = null)
		{
			// Report loading
            List<DailyReport> reports = new List<DailyReport>();
            foreach(string file in Directory.GetFiles("DailyReports")) {
                DailyReport newReport = JsonConvert.DeserializeObject<DailyReport>(File.ReadAllText(file));
                reports.Add(newReport);
            }
			reports = reports.OrderByDescending(grp => grp.DayOfReport.DayOfYear).Reverse().ToList();

            // Data parsing
            double[] messages = new double[reports.Count];
            double[] commands = new double[reports.Count];
            double[] userJoin = new double[reports.Count];
            double[] userLeave = new double[reports.Count];
            string[] xticks = new string[reports.Count];
            for(int i = 0; i < reports.Count; i++) {
                messages[i] += reports[i].MessagesSent;
                commands[i] += reports[i].CommandsRan;
                userJoin[i] += reports[i].UsersJoined;
                userLeave[i] += reports[i].UsersLeft;
            }

            double[] ys = new double[reports.Count];
            for(int i = 0; i < reports.Count; i++) {
                ys[i] = i;
                xticks[i] = (i+1).ToString();
            }
            
            // Plotting
            Plot plt = new Plot(1920, 1080);
            plt.Style(System.Drawing.Color.FromArgb(52, 54, 60), System.Drawing.Color.FromArgb(52, 54, 60), null, System.Drawing.Color.White, System.Drawing.Color.White, System.Drawing.Color.White);
            plt.XLabel("D a y s  S i n c e  C r e a t i o n", null, null, null, 25.5f, false);
            plt.YLabel("G o o d  D a t a", null, null, 25.5f, null, false);
            plt.PlotFillAboveBelow(ys, messages, "Messages", lineWidth: 4, lineColor: System.Drawing.Color.FromArgb(100, 119, 183), fillAlpha: .5, fillColorBelow: System.Drawing.Color.FromArgb(100, 119, 183), fillColorAbove: System.Drawing.Color.FromArgb(100, 119, 183));
            plt.PlotFillAboveBelow(ys, commands, "Command Executions", lineWidth: 4, lineColor: System.Drawing.Color.FromArgb(252, 186, 3), fillAlpha: .5, fillColorBelow: System.Drawing.Color.FromArgb(252, 186, 3), fillColorAbove: System.Drawing.Color.FromArgb(252, 186, 3));
            plt.PlotFillAboveBelow(ys, userJoin, "Users Joined", lineWidth: 4, lineColor: System.Drawing.Color.FromArgb(252, 3, 3), fillAlpha: .5, fillColorBelow: System.Drawing.Color.FromArgb(252, 3, 3), fillColorAbove: System.Drawing.Color.FromArgb(252, 3, 3));
            plt.PlotFillAboveBelow(ys, userLeave, "Users Left", lineWidth: 4, lineColor: System.Drawing.Color.FromArgb(15, 252, 3), fillAlpha: .5, fillColorBelow: System.Drawing.Color.FromArgb(15, 252, 3), fillColorAbove: System.Drawing.Color.FromArgb(15, 252, 3));
            plt.TightenLayout(0, true);
            plt.XTicks(xticks);
            plt.Title("H  a  p  p  y    h  e  a  l  t  h  y    s  e  r  v  e  r", null, null, 45.5f, null, true);
            plt.Legend(true, null, 30, null, null, System.Drawing.Color.FromArgb(100, 52, 54, 60), null, legendLocation.upperRight, shadowDirection.lowerRight, null, null);

            // Save and send
			plt.SaveFig("stats.png");
			if(args != null) {
				await Context.Channel.SendFileAsync("stats.png");
				return;
			}

			// Draw it onto C H A D
			Bitmap chad = new Bitmap(Bitmap.FromFile("chad.png"));
			Bitmap stats = new Bitmap(Bitmap.FromFile("stats.png"));
			using(Graphics canvas = Graphics.FromImage(chad)) {
				canvas.DrawImage(stats, graphPos);
				canvas.Save();
			}

			chad.Save("stats.png");
			await Context.Channel.SendFileAsync("stats.png");
		}

		static Point[] graphPos = new Point[] {
			new Point(900, 500),  // Top left
			new Point(1440, 385), // Top right
			new Point(985, 890),  // Bottom left
		};
	}
}