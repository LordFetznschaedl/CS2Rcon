using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;

namespace CS2Rcon
{
    public class CS2Rcon : BasePlugin
    {
        public override string ModuleName => "CS2Rcon";
        public override string ModuleVersion => "1.1.1";
        public override string ModuleAuthor => "LordFetznschaedl";
        public override string ModuleDescription => "Allows for server commands to be executed from the client";


        private List<ulong> _rconAccessList = new List<ulong>();
        private FileSystemWatcher? _fileWatcher = null;
        private DateTime _fileWatcherLastFileWriteTime = DateTime.MinValue;
        private string _rconAccessPath = string.Empty;   
        private const string RCON_NO_ACCESS = "YOU DONT HAVE PERMISSION TO USE THIS COMMAND!";

        public override void Load(bool hotReload)
        {
            this.Log(PluginInfo());
            this.Log(this.ModuleDescription);

            this._rconAccessPath = Path.Join(this.ModuleDirectory, "rconAllow.cfg");

            this.ReloadAccessSteamIds();
            this.SetupRconAccessFileWatcher();

        }

        public override void Unload(bool hotReload)
        {
            if(this._fileWatcher != null)
            {
                this._fileWatcher.Dispose();
            }

            base.Unload(hotReload);
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
                this.Log($"{player.PlayerName:Server} [{player.SteamID}] is trying to access RCON but has no permission!");
                player.PrintToChat(RCON_NO_ACCESS);
                player.PrintToConsole(RCON_NO_ACCESS);

                return;
            }
            
            if(command.ArgCount <= 0)
            {
                this.PrintToPlayerOrServer($"{player?.PlayerName??"Server"} [{player?.SteamID??0}] executing RCON without ARGS is not possible!");
                return; 
            }
            
            var serverCommand = command.ArgString;

            Server.ExecuteCommand(serverCommand);

            this.Log($"Player {player?.PlayerName??"Server"} [{player?.SteamID??0}] executed rcon! [${serverCommand}]");
        }

        [ConsoleCommand("css_rconreload", "Reloads who has access to rcon!")]
        public void OnRconReloadAccessSteamIds(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && !this._rconAccessList.Contains(player.SteamID))
            {
                this.Log($"{player.PlayerName} [{player.SteamID}] is trying to reload the access steam ids!");
                player.PrintToChat(RCON_NO_ACCESS);
                player.PrintToConsole(RCON_NO_ACCESS);

                return;
            }

            this.ReloadAccessSteamIds();

            this.Log($"Player {player?.PlayerName??"Server"} [{player?.SteamID??0}] executed reload access steam ids!");
            this.PrintToPlayerOrServer("Reload was successful");
        }

        [ConsoleCommand("css_rconwho", "List all SteamId64 that have access to the RCON commands in the console!")]
        public void OnRconWho(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && !this._rconAccessList.Contains(player.SteamID))
            {
                this.Log($"{player.PlayerName} [{player.SteamID}] is trying to access the list of allowed steam ids!");
                player.PrintToChat(RCON_NO_ACCESS);
                player.PrintToConsole(RCON_NO_ACCESS);
                return;
            }

            this._rconAccessList.ForEach(steamId => this.PrintToPlayerOrServer($"SteamId64: {steamId}", player));

            this.Log($"Player {player?.PlayerName??"Server"} [{player?.SteamID??0}] executed reload access steam ids!");
        }

        [ConsoleCommand("css_rconinfo", "This command prints the plugin information")]
        public void OnRconInfo(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                this.Log(PluginInfo());
                return;
            }

            player.PrintToChat(PluginInfo());
            player.PrintToConsole(PluginInfo());
        }

        private void ReloadAccessSteamIds()
        {
            this.Log($"Reloading... [{this._rconAccessPath}]");

            if(this._rconAccessList.Any())
            {
                this._rconAccessList = new List<ulong>();
            }

            if (File.Exists(this._rconAccessPath))
            {
                var lines = File.ReadAllLines(this._rconAccessPath).ToList().Where(x => !x.StartsWith('#'));

                foreach (var line in lines)
                {
                    if (ulong.TryParse(line, out var steamId))
                    {
                        this.Log($"SteamId64: {steamId}");
                        this._rconAccessList.Add(steamId);
                    }
                }

                this._fileWatcherLastFileWriteTime = File.GetLastWriteTime(this._rconAccessPath);
            }
        }

        private void SetupRconAccessFileWatcher()
        {
            this.Log($"Setting up FileWatcher for {this._rconAccessPath}");

            if (this._fileWatcher != null)
            {
                this._fileWatcher.Dispose();
            }

            this._fileWatcher = new FileSystemWatcher()
            {
                Path = Path.GetDirectoryName(this._rconAccessPath) ?? this.ModulePath,
                Filter = Path.GetFileName(this._rconAccessPath),
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true,
            };

            this._fileWatcher.Changed += this.OnFileChanged;
            
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            var lastWriteTime = File.GetLastWriteTime(this._rconAccessPath);

            if (lastWriteTime.Ticks - this._fileWatcherLastFileWriteTime.Ticks < 100000)
            {
                return;
            }
            this._fileWatcherLastFileWriteTime = lastWriteTime;

            this.Log($"Change detected on {e.FullPath}. Reloading rconAccess...");

            this.ReloadAccessSteamIds();

        }

        private string PluginInfo()
        {
            return $"Plugin: {this.ModuleName} - Version: {this.ModuleVersion} by {this.ModuleAuthor}";
        }

        private void PrintToPlayerOrServer(string message, CCSPlayerController? player = null)
        {
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