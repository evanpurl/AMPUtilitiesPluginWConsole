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
        public static void GenerateModListFile(ITorchSession session)
        {
            string sandboxPath = Path.Combine(session.KeenSession.CurrentPath, "Sandbox_config.sbc");
            if (!File.Exists(sandboxPath))
            {
                AmpUtilities.Log.Warn("Sandbox_config.sbc not found.");
                return;
            }

            var persistentSandbox = Persistent<MyObjectBuilder_WorldConfiguration>.Load(sandboxPath);
            var mods = persistentSandbox.Data.Mods;
            AmpUtilities.Log.Info("Modlist on load: " + mods.Count() + " mods found.");

            var modListLines = new List<string>();
            foreach (var mod in mods)
            {
                var modId = mod.GetWorkshopId().Id;
                var service = mod.PublishedServiceName ?? "Steam"; // fallback if null
                modListLines.Add($"{modId}-{service}");
            }

            // Write to modlist.txt in the same folder
            string outputPath = Path.Combine(AmpUtilities.MainDirectory, "modlist.txt");
            File.WriteAllLines(outputPath, modListLines);

            AmpUtilities.Log.Info($"modlist.txt written with {modListLines.Count} entries.");
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
