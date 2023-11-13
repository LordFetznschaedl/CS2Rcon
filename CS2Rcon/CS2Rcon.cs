using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2Rcon
{
    [MinimumApiVersion(28)]
    public class CS2Rcon : BasePlugin
    {
        public override string ModuleName => "CS2Rcon";
        public override string ModuleVersion => "1.2.0";
        public override string ModuleAuthor => "LordFetznschaedl";
        public override string ModuleDescription => "Allows for server commands to be executed from the client using !rcon";

        public override void Load(bool hotReload)
        {
            this.Log(PluginInfo());
            this.Log(this.ModuleDescription);
        }

        [ConsoleCommand("css_rcon", "Chat rcon as we know it!")]
        [RequiresPermissions("@css/rcon")]
        public void OnRcon(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                this.Log("Command has been called by the server.");
            }
            
            if(command.ArgCount <= 0)
            {
                this.PrintToPlayerOrServer($"{player?.PlayerName??"Server"} [{player?.SteamID??0}] executing RCON without ARGS is not possible!");
                return; 
            }
            
            var serverCommand = command.ArgString;

            Server.ExecuteCommand(serverCommand);

            this.Log($"Player {player?.PlayerName??"Server"} [{player?.SteamID??0}] executed rcon! [${serverCommand}]");
            this.PrintToPlayerOrServer($"Command executed:{ChatColors.BlueGrey}{serverCommand}", player);
        }

        [ConsoleCommand("css_rconinfo", "This command prints the plugin information")]
        public void OnRconInfo(CCSPlayerController? player, CommandInfo command)
        {
            this.PrintToPlayerOrServer(this.PluginInfo(), player);
        }

        private string PluginInfo()
        {
            return $"Plugin: {this.ModuleName} - Version: {this.ModuleVersion} by {this.ModuleAuthor}";
        }

        private void PrintToPlayerOrServer(string message, CCSPlayerController? player = null)
        {
            message = $"[{ChatColors.Red}{this.ModuleName}{ChatColors.White}] " + message;

            if(player != null) 
            {
                player.PrintToConsole(message);
                player.PrintToChat(message);
            }
            else
            {
                this.Log(message);
            }
        }

        private void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{this.ModuleName}] {message}");
            Console.ResetColor();
        }
    }
}