using Sandbox.Game.Entities.Cube;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.API.Session;
using Torch.Managers.PatchManager;
using Torch.Mod;
using Torch.Mod.Messages;
using Torch.Server;

namespace AMPUtilitiesPurlsWay.Patches
{
    [PatchShim]
    public static class RestartPatcher
    {
        internal static readonly MethodInfo restartpatchermethod = typeof(TorchServer).GetMethod("Restart", BindingFlags.Instance | BindingFlags.Public)
                                                                       ?? throw new System.Exception("Failed to find method Restart");

        internal static readonly MethodInfo prefixMethod = typeof(RestartPatcher).GetMethod(nameof(RestartPatch), BindingFlags.Static | BindingFlags.NonPublic)
                                                           ?? throw new System.Exception("Failed to find method RestartPAtch");
        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(restartpatchermethod).Prefixes.Add(prefixMethod);
        }
        private static bool RestartPatch(TorchServer __instance, bool save)
        {
            if (__instance.Config.DisconnectOnRestart)
            {
                ModCommunication.SendMessageToClients(new JoinServerMessage("0.0.0.0:25555"));
                AmpUtilities.Log.Info("Ejected all players from server for restart.");
            }
            if (__instance.IsRunning && save)
                __instance.Save().ContinueWith(KillProc, __instance, TaskContinuationOptions.RunContinuationsAsynchronously);

            KillProc(null, __instance);
            return false;
        }

        public static void KillProc(Task<GameSaveResult> task, object torch0)
        {
            Process.GetCurrentProcess().Kill();
        }
    }

    
}
