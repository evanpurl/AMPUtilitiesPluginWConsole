using NLog;
using Sandbox.ModAPI;
using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Commands;
using Torch.Server.Views;
using Torch.Session;
using Torch.Views;
using VRage.Game;
using VRage.Utils;
using AMPUtilitiesPurlsWay.Utils;

namespace AMPUtilitiesPurlsWay
{
    public class AmpUtilities : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Persistent<Config> _config = null!;
        private static Persistent<Utils.Config> _modlist = null!;
        public static Persistent<Utils.Config> PubModlist => _modlist;

        public static string MainDirectory { get; private set; }
        private Thread _inputThread;
        private bool _running = true;
        private static CommandManager _commandManager;
        private static IChatManagerServer _chatManagerServer;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            _config = Persistent<Config>.Load(Path.Combine(StoragePath, "AMPUtilitiesPlugin.cfg"));
            MainDirectory = StoragePath;
            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
        }

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {

            switch (state)
            {
                case TorchSessionState.Loaded:
                    Log.Info("Session Loaded!");
                    Modlist.GenerateModListFile(session);
                    break;

                case TorchSessionState.Unloading:
                    Log.Info("Session Unloading!");
                    string sandboxPath = Path.Combine(session.KeenSession.CurrentPath, "Sandbox_config.sbc");
                    if (!File.Exists(sandboxPath))
                    {
                        Log.Warn("Sandbox_config.sbc not found.");
                        break;
                    }
                    //var persistentSandbox = Persistent<MyObjectBuilder_WorldConfiguration>.Load(sandboxPath);
                    //var checkpoint = persistentSandbox.Data;
                    //checkpoint.Mods.Clear();
                    //foreach (var mod in _modlist.Data.Mods)
                    //{
                    //    ulong id;
                    //    string name = mod;
                    //    if (mod.StartsWith("mod.io/"))
                    //    {
                    //        id = 0;
                    //    }
                    //    else if (ulong.TryParse(mod, out var parsedId))
                    //    {
                    //        name = $"{mod}.sbm";
                    //        id = parsedId;
                    //    }
                    //    else
                    //    {
                    //        Log.Warn($"Skipping invalid mod entry: {mod}");
                    //        continue;
                    //    }

                    //    checkpoint.Mods.Add(new MyObjectBuilder_Checkpoint.ModItem
                    //    {
                    //        Name = name,
                    //        PublishedFileId = id
                    //    });
                    //}

                    //persistentSandbox.Save(sandboxPath);
                    //Log.Info("Sandbox_config.sbc mod list updated from Modlist.cfg.");
                    break;
                }
            }

        public UserControl GetControl() => new PropertyGrid
        {

        };

        private async Task ReadInputLoopAsync()
        {
            while (_running)
            {
                try
                {
                    string line = await ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    Log.Info($"[StdioPlugin] Received Input: {line}");
                    Thread CurrentThread = Thread.CurrentThread;
                    if (CurrentThread != MyUtils.MainThread)
                    {
                        MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                        {
                            try
                            {
                                if (line.StartsWith("!"))
                                {
                                    RunCommand(line);
                                }
                                else
                                {
                                    SendChatMessage("Server", line);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warn($"Error when invoking {ex}");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[StdioPlugin] STDIN error: {ex.Message}");
                }
            }
        }

        public static Task<string> ReadLineAsync()
        {
            return Task.Run(() => Console.ReadLine());
        }


        public void SendChatMessage(string author, string message)
        {
            try
            {
                if (_chatManagerServer != null)
                    _chatManagerServer.SendMessageAsOther(author, message, VRageMath.Color.Blue);
                else
                    Log.Info($"Can't find Chat Manager");
            }
            catch (Exception ex)
            {
                Log.Info($"Error sending chat message {ex.Message}");
            }
        }

        public void RunCommand(string commandText)
        {
            try
            {

                if (_commandManager == null)
                {
                    Log.Info($"Command is null");
                    return;
                }
                if (_commandManager.Commands == null)
                {
                    Log.Info($"Command tree is null");
                    return;
                }
                string argsText;

                if (commandText.StartsWith("!"))
                    commandText = commandText.Substring(1);

                var command = _commandManager.Commands.GetCommand(commandText, out argsText);

                if (command != null)
                {
                    var argsList = argsText.Split(' ').ToList();
                    var context = new CommandContext(Torch, this, 0, argsText, argsList);
                    command.TryInvoke(context);
                } else
                {
                    Log.Info($"Command later is null");
                }
            }
            catch (Exception ex)
            {
                Log.Info($"Error running command: {ex}");
            }
        }

    }
}