using HarmonyLib;
using System;
using NuclearCruiser.Utils;

namespace NuclearCruiser.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public static class StartOfRoundPatch
    {
        [HarmonyPatch(nameof(StartOfRound.Awake))]
        [HarmonyPrefix]
        public static void AwakePrefix()
        {
            Network.NetworkHandler.SpawnNetworkHandler();
        }

        [HarmonyPatch(nameof(StartOfRound.SyncAlreadyHeldObjectsServerRpc))]
        [HarmonyPostfix]
        public static void SyncAlreadyHeldObjectsServerRpc()
        {
            VehicleController vc = StartOfRound.Instance.attachedVehicle;
            if (!vc) return;
            CruiserNuker nuker = vc.gameObject.GetComponent<CruiserNuker>();
            if (!nuker) return;
            Network.NetworkHandler.Instance.AddCruiserNukerClientRpc(vc.NetworkObject);
        }

        [HarmonyPatch(nameof(StartOfRound.LoadAttachedVehicle))]
        [HarmonyPostfix]
        public static void LoadAttachedVehicle_Postfix() 
        {
            try
            {
                if (ES3.KeyExists(MyPluginInfo.PLUGIN_NAME + NuclearCruiser.IsNuclear, GameNetworkManager.Instance.currentSaveFileName))
                {
                    bool cruiserState = ES3.Load<bool>(MyPluginInfo.PLUGIN_NAME + NuclearCruiser.IsNuclear, GameNetworkManager.Instance.currentSaveFileName);
                    if (cruiserState && !StartOfRound.Instance.attachedVehicle.gameObject.GetComponent<CruiserNuker>())
                    {
                        Network.NetworkHandler.Instance.AddCruiserNukerClientRpc(StartOfRound.Instance.attachedVehicle.NetworkObject);
                    }
                }
            }
            catch(Exception e)
            {
                NuclearCruiser.Logger.LogError($"Failed to load nuclear cruiser data: {e}");
            }     
        }
    }
}
