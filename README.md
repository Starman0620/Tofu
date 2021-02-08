# Tofu
Tofu is a Discord bot designed specifically for use in the [Cerro Gordo Discord server](https://discord.gg/J5UR49wAaD). As of now, it only has rather basic features but in the future more advanced features will be added as they are needed.

## Usage/Build instructions
1. Clone the repository: ``git clone https://github.com/Starman0620/Tofu.git`` and move into the project directory: ``cd Tofu/TofuBot``
2. Build the source code: ``dotnet build -c Release -r linux-x64`` (If you're building on Windows, the -r option should be win-x64) 
3. Change into the build directory: ``cd bin/Release/netcoreapp3.1/linux-x64/`` (If you're on Windows, you'll once again need to replace linux-x64 with win-x64)
4. Run the bot: ``./TofuBot`` or just ``TofuBot`` for Windows. This will generate a blank configuration file for you.
5. Edit the ``config.json``  file and add your token into the token field, and an API key for [Weather API](https://www.weatherapi.com/) if you want the weather command to work.
6. Run the bot once more, as before. Once it has started up (It'll output "Ready" to the terminal), you should be good to go into Discord and use it.
