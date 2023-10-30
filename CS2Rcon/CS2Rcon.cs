using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace CS2Rcon
{
    public class CS2Rcon : BasePlugin
    {
        public override string ModuleName => "CS2Rcon";
        public override string ModuleVersion => "1.0.1";

        private List<ulong> _rconAccessList = new List<ulong>();
        private const string RCON_NO_ACCESS = "YOU DONT HAVE PERMISSION TO USE THIS COMMAND!";
        public override void Load(bool hotReload)
        {
            this.Log(PluginInfo());

            this.ReloadAccessSteamIds();
        }

        [ConsoleCommand("css_rcon", "Rcon as we know it!")]
        public void OnRcon(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                this.Log("Command has been called by the server.");
            }

            if(player != null && !this._rconAccessList.Contains(player.SteamID)) 
            {
                this.Log($"{player.PlayerName} [{player.SteamID}] is trying to access RCON but has no permission!");
                player.PrintToCenter(RCON_NO_ACCESS);
                player.PrintToChat(RCON_NO_ACCESS);
                player.PrintToConsole(RCON_NO_ACCESS);

                return;
            }
            
            if(command.ArgCount <= 0)
            {
                this.Log($"{player?.PlayerName} [{player?.SteamID}] executing RCON without ARGS!");
            }
            
            var serverCommand = command.ArgString;

            Server.ExecuteCommand(serverCommand);

            this.Log($"Player {player?.PlayerName} [{player?.SteamID}] executed rcon! [${serverCommand}]");
        }

        [ConsoleCommand("css_rconreload", "Reloads who has access to rcon!")]
        public void OnRconReloadAccessSteamIds(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                this.Log("Command has been called by the server.");
            }

            if (player != null && !this._rconAccessList.Contains(player.SteamID))
            {
                this.Log($"{player.PlayerName} [{player.SteamID}] is trying to reload the access steam ids!");
                player.PrintToCenter(RCON_NO_ACCESS);
                player.PrintToChat(RCON_NO_ACCESS);
                player.PrintToConsole(RCON_NO_ACCESS);

                return;
            }

            this.ReloadAccessSteamIds();

            this.Log($"Player {player?.PlayerName} [{player?.SteamID}] executed reload access steam ids!");
        }

        [ConsoleCommand("css_rconinfo", "This command prints the plugin information")]
        public void OnRconInfo(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                this.Log("Command has been called by the server.");
                return;
            }

            player.PrintToChat(PluginInfo());
            player.PrintToConsole(PluginInfo());
        }

        private void ReloadAccessSteamIds()
        {
            var path = Path.Join(this.ModuleDirectory, "rconAllow.cfg");
            this.Log($"rconAllow.cfg Path: {path}");

            if(this._rconAccessList.Any())
            {
                this._rconAccessList = new List<ulong>();
            }

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path).ToList().Where(x => !x.StartsWith('#'));

                foreach (var line in lines)
                {
                    if (ulong.TryParse(line, out var steamId))
                    {
                        this._rconAccessList.Add(steamId);
                    }
                }
            }
        }

        private string PluginInfo()
        {
            return $"Plugin: {ModuleName} - Version: {ModuleVersion} by LordFetznschaedl";
        }

        private void Log(string message)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[CS2Rcon] " + message);
            Console.ResetColor();
        }
    }
}