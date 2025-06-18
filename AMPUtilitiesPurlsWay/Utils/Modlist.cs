using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace AMPUtilitiesPurlsWay.Utils
{
    public class Modlist
    {
        public Dictionary<string, ulong> GetModlistonload()
        {
            List<MyObjectBuilder_Checkpoint.ModItem> mods = MySession.Static.Mods;
            Dictionary<string, ulong> modIds = new Dictionary<string, ulong>();
            foreach (var mod in mods)
            {
                modIds[mod.PublishedServiceName] = mod.GetWorkshopId().Id;
            }
            return modIds;

        }

        public bool SetModlistonUnload()
        {
            return true;
        }
    }
}
