using NLog;
using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Commands;
using Torch.Session;
using Torch.Views;

namespace AMPUtilitiesPurlsWay
{
    public class AmpUtilities : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Persistent<Config> _config = null!;
        private Thread _inputThread;
        private bool _running = true;
        private CommandManager _commandManager;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            _config = Persistent<Config>.Load(Path.Combine(StoragePath, "AMPUtilitiesPlugin.cfg"));
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
                    _commandManager = Torch.Managers.GetManager<CommandManager>();
                    _inputThread = new Thread(ReadInputLoop)
                    {
                        IsBackground = true
                    };
                    _inputThread.Start();
                    break;

                case TorchSessionState.Unloading:
                    Log.Info("Session Unloading!");
                    break;
            }
        }

        public UserControl GetControl() => new PropertyGrid
        {

        };

        private void ReadInputLoop()
        {
            while (_running)
            {
                try
                {
                    string line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    Log.Info($"[StdioPlugin] Received Input: {line}");
                    _commandManager?.HandleCommandFromServer(line);
                }
                catch (Exception ex)
                {
                    Log.Error($"[StdioPlugin] STDIN error: {ex.Message}");
                }
            }
        }
    }
}