using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace RandomMoonFX
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("StartGame")]
        public static bool RandomizeMoonPatch()
        {
            if (Plugin.instance.CompanyRoutingMode == Plugin.CompanyRM.Manual && Plugin.instance.LastDayOfQuota())
            {
                return true;
            }
            if (Plugin.instance.IsStarting)
            {
                Plugin.instance.IsStarting = false;
                return true;
            }
            else
            {
                Plugin.instance.RouteRandomPlanet();
                return false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("TravelToLevelEffects")]
        public static IEnumerator StartMoonPatch(IEnumerator result)
        {
            while (result.MoveNext())
            {
                yield return result.Current;
            }
            if (Plugin.instance.CompanyRoutingMode == Plugin.CompanyRM.Manual && Plugin.instance.LastDayOfQuota())
            {
                yield break;
            }
            else if (Plugin.instance.IsStarting)
            {
                StartMatchLever lever = Object.FindObjectOfType<StartMatchLever>();
                lever.triggerScript.interactable = false;
                yield return new WaitForSeconds(Plugin.instance.AnimationTime);
                lever.triggerScript.interactable = true;
                Plugin.instance.StartRandomPlanet();
            }
        }
    }


    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("BeginHoldingInteractOnLever")]
        public static bool DisabledLastDayWarningPatch()
        {
            if (StartOfRound.Instance.CanChangeLevels() && Plugin.instance.LastDayOfQuota() && Plugin.instance.CompanyRoutingMode != Plugin.CompanyRM.Manual)
            {
                return false;
            }
            return true;
        }

        // COMPATIBILITY PATCH WITH LETHALFIXES MOD, NOT NEEDED IN VANILLA
        [HarmonyPostfix]
        [HarmonyPatch("BeginHoldingInteractOnLever")]
        public static void DisabledLongHoldTime(ref StartMatchLever __instance)
        {
            if (TimeOfDay.Instance.daysUntilDeadline <= 0 && __instance.playersManager.inShipPhase && StartOfRound.Instance.currentLevel.planetHasTime
                && Plugin.instance.LastDayOfQuota() && Plugin.instance.CompanyRoutingMode != Plugin.CompanyRM.Manual)
            {
                __instance.triggerScript.timeToHold = 0.7f;
            }
        }
    }


    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void AwakePatch()
        {
            Plugin.instance.terminal = Object.FindObjectOfType<Terminal>();
        }

        [HarmonyPrefix]
        [HarmonyPatch("LoadNewNode")]
        public static void LoadNewNodePrePatch(ref TerminalNode node)
        {
            if (Plugin.instance.terminal != null && node != null && node.buyRerouteToMoon == -2)
            {
                Plugin.instance.terminalCostOfItems = Plugin.instance.terminal.totalCostOfItems;
                Plugin.instance.terminal.totalCostOfItems = 0;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("LoadNewNode")]
        public static void LoadNewNodePostPatch(ref TerminalNode node)
        {
            if (Plugin.instance.terminal != null && node != null && Plugin.instance.terminalCostOfItems != -5)
            {
                Plugin.instance.terminal.totalCostOfItems = 0;
                Plugin.instance.terminalCostOfItems = -5;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("LoadNewNodeIfAffordable")]
        public static void LoadNewNodeIfAffordablePatch(ref TerminalNode node)
        {
            if (Plugin.instance.terminal != null && node != null && node.buyRerouteToMoon != -1)
            {
                node.itemCost = 0;
            }
        }
    }
}
