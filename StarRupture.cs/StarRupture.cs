using System;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using WindowsGSM.GameServer.Engine;
using System.IO;



namespace WindowsGSM.Plugins
{
    public class StarRupture : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.StarRupture", // WindowsGSM.XXXX
            author = "fantasticdave", // thanks to sh1ny for the asa plugin i used that as the base for this!
            description = "WindowsGSM plugin for supporting StarRupture Dedicated Server",
            version = "1.21",
            url = "https://github.com/fantasticdave/WindowsGSM.StarRupture/", // Github repository link (Best practice)
            color = "#34c9eb" // Color Hex
        };

        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true; // As of 25.18, login is no longer needed. Source: https://discord.com/channels/729837326120910915/735188487615283232/1167603768091754537
        public override string AppId => "3809400"; // Game server appId

        // - Standard Constructor and properties
        public StarRupture(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error;


        // - Game server Fixed variables
        public string StartPath = @"StarRupture\Binaries\Win64\StarRuptureServerEOS-Win64-Shipping.exe"; // Game server start path
        public string FullName = "StarRupture Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 2; // This tells WindowsGSM how many ports should skip after installation
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()


        // - Game server default values
        public string Port = "7777"; // Default port
        public string QueryPort = "27015"; // Default query port
        public string Defaultmap = "Default"; // Default map name
        public string Maxplayers = "10"; // Default maxplayers
        // Additional server start parameter; unused but kept for future custom flags
        public string Additional = "";


        private void CreateServerSettingsFile()
        {
            // Build DSSettings.txt using WindowsGSM inputs and optional ServerParam overrides
            var settingsPath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, "DSSettings.txt");

            string sessionName = GetParamOrDefault("SessionName", string.IsNullOrWhiteSpace(_serverData.ServerName)
                ? $"StarRupture-{_serverData.ServerID}"
                : _serverData.ServerName);

            string saveGameInterval = GetParamOrDefault("SaveGameInterval", "300");
            string startNewGame = GetParamOrDefault("StartNewGame", "true");
            string loadSavedGame = GetParamOrDefault("LoadSavedGame", "false");
            string saveGameName = GetParamOrDefault("SaveGameName", "AutoSave0.sav");

            // Avoid conflicting start/load flags
            if (bool.TryParse(loadSavedGame, out var loadFlag) && loadFlag)
            {
                startNewGame = "false";
            }

            var json = new StringBuilder()
                .AppendLine("{")
                .AppendLine($"  \"SessionName\": \"{sessionName}\",")
                .AppendLine($"  \"SaveGameInterval\": \"{saveGameInterval}\",")
                .AppendLine($"  \"StartNewGame\": \"{startNewGame}\",")
                .AppendLine($"  \"LoadSavedGame\": \"{loadSavedGame}\",")
                .AppendLine($"  \"SaveGameName\": \"{saveGameName}\"")
                .AppendLine("}");

            try
            {
                File.WriteAllText(settingsPath, json.ToString());
            }
            catch (Exception ex)
            {
                Error = $"Failed to write DSSettings.txt: {ex.Message}";
            }
        }

        private string GetParamOrDefault(string key, string defaultValue)
        {
            var value = ExtractParamValue(_serverData.ServerParam, key);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static string ExtractParamValue(string paramString, string key)
        {
            if (string.IsNullOrWhiteSpace(paramString))
            {
                return null;
            }

            var tokens = paramString.Split(new[] { '?', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var cleaned = token.Trim().TrimStart('-', '/');
                var parts = cleaned.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    continue;
                }

                if (parts[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return parts[1].Trim().Trim('"');
                }
            }

            return null;
        }


        // - Create a default cfg for the game server after installation
        public void CreateServerCFG()
        {
            CreateServerSettingsFile();
        }

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string shipExePath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(shipExePath))
            {
                Error = $"{Path.GetFileName(shipExePath)} not found ({shipExePath})";
                return null;
            }

            CreateServerSettingsFile();

            var param = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(_serverData.ServerName))
                param.Append($" StarRupture -Log ");


            if (!string.IsNullOrWhiteSpace(_serverData.ServerName))
                param.Append($" -ServerName=\"\"\"{_serverData.ServerName}\"\"\"");

            if (!string.IsNullOrWhiteSpace(_serverData.ServerIP))
                param.Append($" -MULTIHOME={_serverData.ServerIP}");

            if (!string.IsNullOrWhiteSpace(_serverData.ServerPort))
                param.Append($" -Port={_serverData.ServerPort}");

            if (!string.IsNullOrWhiteSpace(_serverData.ServerQueryPort))
                param.Append($" -QueryPort={_serverData.ServerQueryPort}");

            if (!string.IsNullOrWhiteSpace(_serverData.ServerParam))
                if(_serverData.ServerParam.StartsWith("?"))
                    param.Append($"{_serverData.ServerParam}");
                else if (_serverData.ServerParam.StartsWith("-"))
                    param.Append($" {_serverData.ServerParam}");

            Process p;
            if (!AllowsEmbedConsole)
            {
                p = new Process
                {
                    StartInfo =
                    {
                        FileName = shipExePath,
                        Arguments = param.ToString(),
                        WindowStyle = ProcessWindowStyle.Minimized,
                        UseShellExecute = false
                    },
                    EnableRaisingEvents = true
                };
                p.Start();
            }
            else
            {
                p = new Process
                {
                    StartInfo =
                    {
                        FileName = shipExePath,
                        Arguments = param.ToString(),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = false,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };
                var serverConsole = new Functions.ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }

            return p;
        }


        // - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(async () =>
            {
                // Prefer graceful Ctrl+C shutdown for UE-based dedicated servers
                try
                {
                    Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                    Functions.ServerConsole.SendWaitToMainWindow("^C");
                    Functions.ServerConsole.SendWaitToMainWindow("{ENTER}");
                    await Task.Delay(6000);
                }
                catch
                {
                    // Fallback to closing the main window if Ctrl+C fails
                    try { p.CloseMainWindow(); } catch { /* ignore */ }
                }
            });
        }

        public async Task<Process> Install()
        {
            var steamCMD = new Installer.SteamCMD();
            Process p = await steamCMD.Install(_serverData.ServerID, string.Empty, AppId, true, loginAnonymous);
            Error = steamCMD.Error;

            return p;
        }

        public async Task<Process> Update(bool validate = false, string custom = null)
        {
            var (p, error) = await Installer.SteamCMD.UpdateEx(_serverData.ServerID, AppId, validate, custom: custom, loginAnonymous: loginAnonymous);
            Error = error;
            return p;
        }

        public bool IsInstallValid()
        {
            return File.Exists(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath));
        }

        public bool IsImportValid(string path)
        {
            string importPath = Path.Combine(path, StartPath);
            Error = $"Invalid Path! Fail to find {Path.GetFileName(StartPath)}";
            return File.Exists(importPath);
        }

        public string GetLocalBuild()
        {
            var steamCMD = new Installer.SteamCMD();
            return steamCMD.GetLocalBuild(_serverData.ServerID, AppId);
        }

        public async Task<string> GetRemoteBuild()
        {
            var steamCMD = new Installer.SteamCMD();
            return await steamCMD.GetRemoteBuild(AppId);
        }

    }
}
