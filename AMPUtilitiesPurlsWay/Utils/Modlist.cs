using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Torch;
using Torch.API.Session;
using VRage.Game;

namespace AMPUtilitiesPurlsWay.Utils
{
    public static class Modlist
    {
        public static Dictionary<ulong, string>? GetModlistonload(ITorchSession session)
        {
            string sandboxPath = Path.Combine(session.KeenSession.CurrentPath, "Sandbox_config.sbc");
            if (!File.Exists(sandboxPath))
            {
                AmpUtilities.Log.Warn("Sandbox_config.sbc not found.");
                return null;
            }
            var persistentSandbox = Persistent<MyObjectBuilder_WorldConfiguration>.Load(sandboxPath);
            AmpUtilities.Log.Info("Modlist on load: " + persistentSandbox.Data.Mods.Count() + " mods found.");
            Dictionary<ulong, string> modIds = new Dictionary<ulong, string>();
            foreach (var mod in persistentSandbox.Data.Mods)
            {
                modIds[mod.GetWorkshopId().Id] = mod.PublishedServiceName;
            }
            return modIds;

        }

        public static Dictionary<string, ulong> GetModlistCfg()
        {
            List<MyObjectBuilder_Checkpoint.ModItem> mods = MySession.Static.Mods;
            Dictionary<string, ulong> modIds = new Dictionary<string, ulong>();
            foreach (var mod in mods)
            {
                modIds[mod.PublishedServiceName] = mod.GetWorkshopId().Id;
            }
            return modIds;

        }

        public static void UpdateModList(Dictionary<ulong, string> newMods)
        {
            if (newMods == null)
            {
                AmpUtilities.Log.Warn("No mod list found to update.");
                return;
            }
            List<string> strings = new List<string>();
            List<string> services = new List<string>();
            AmpUtilities.PubModlist.Data.Mods.Clear();
            foreach (var mod in newMods)
            {
                strings.Add($"{mod.Key}");
                services.Add(mod.Value);
            }
            AmpUtilities.Log.Warn("Updating internal mod list with " + strings.Count + " mods.");    
            AmpUtilities.PubModlist.Data.Mods = strings;
            AmpUtilities.PubModlist.Data.Service = services;

            AmpUtilities.PubModlist.Save();
        }


    }
}
