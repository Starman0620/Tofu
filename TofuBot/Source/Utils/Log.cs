using System;
using System.IO;

using Discord.WebSocket;

namespace TofuBot.Utils
{
    internal class Log
    {
        public static void Write(string text, bool print = true)
        {
			// Setup
            if(!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
            string message =$"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}] {text}";

			// Print, send and write the log
            if(print) Console.WriteLine(message);
            File.AppendAllText($"Logs/{DateTime.Now.ToShortDateString().Replace("\\", "-").Replace("/", "-")}.log", message+'\n'); // Replaces here are for Windows and other date formats
			((SocketTextChannel)Bot.client.GetChannel(Bot.config.LogChannel)).SendMessageAsync(message);
        }
    }
}