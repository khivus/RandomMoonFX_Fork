using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;


namespace RandomMoonFX
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.github.darmuh.LethalConstellations", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("butterystancakes.lethalcompany.chameleon", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("CelestialTint", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "zigzag.randommoonfx";
        const string NAME = "RandomMoonFX";
        const string VERSION = "1.4.2";

        //internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(GUID);

        public static Plugin instance;
        private readonly Harmony harmony = new Harmony(GUID);
        internal static Config config { get; private set; } = null!;

        private readonly int GordionID = 3;  // vanilla Company moon ID is always 3, used for fallback errors
        public float AnimationTime = 1.5f;
        public bool IsStarting = false;
        public CompanyRM CompanyRoutingMode;
        public string SelectedCompanyName = "";  // used if CompanyRoutingMode Select
        public int SelectedCompanyID = -1;  // used if CompanyRoutingMode Select

        public Terminal? terminal;
        public int terminalCostOfItems = -5;
        public bool constellationsCompatibility = false;

        public List<string> VisitedMoons = new List<string>();
        public List<SelectableLevel> CompanyMoons = new List<SelectableLevel>();  // used if CompanyRoutingMode Random

        public enum CompanyRM
        {
            Random,
            Select,
            Manual
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
        void Awake()
        {
            instance = this;
            SetParameters();
            if (config.ActivateRandomMoons.Value)
            {
                harmony.CreateClassProcessor(typeof(StartOfRoundPatch), true).Patch();
                harmony.CreateClassProcessor(typeof(StartMatchLeverPatch), true).Patch();
            }
            if (config.ActivateFreeMoons.Value)
            {
                harmony.CreateClassProcessor(typeof(TerminalPatch), true).Patch();
            }
            Logger.LogInfo("RandomMoonFX is loaded !");
        }

        private void SetParameters()
        {
            config = new Config(base.Config);
            config.SetupCustomConfigs();

            CompanyRoutingMode = config.CompanyRoutingMode.Value switch
            {
                "Random" => CompanyRM.Random,
                "Select" => CompanyRM.Select,
                "Manual" => CompanyRM.Manual,
                _ => CompanyRM.Random
            };
            SelectedCompanyName = Utils.GetNormalizedMoonName(config.SelectedCompanyName.Value);

            if (config.ChameleonAnimation.Value && Chainloader.PluginInfos.ContainsKey("butterystancakes.lethalcompany.chameleon"))
                AnimationTime = 7.5f;
            if (config.CelestialTintAnimation.Value && Chainloader.PluginInfos.ContainsKey("CelestialTint"))
                AnimationTime = 4f;
            if (config.AnimationTimeOverride.Value >= 0f)
                AnimationTime = config.AnimationTimeOverride.Value;
            if (config.ConstellationsCheck.Value && Chainloader.PluginInfos.ContainsKey("com.github.darmuh.LethalConstellations"))
                constellationsCompatibility = true;
        }

        public bool LastDayOfQuota()
        {
            if (Utils.IsLastDayRandom())
                return false;
            if (config.QuotaCheck.Value)
                return TimeOfDay.Instance.daysUntilDeadline == 0 && TimeOfDay.Instance.profitQuota > TimeOfDay.Instance.quotaFulfilled;
            else
                return TimeOfDay.Instance.daysUntilDeadline == 0;
        }

        public void RouteRandomPlanet()
        {
            Utils.SearchCompanyMoons();
            if (LastDayOfQuota())
            {
                int levelID = GordionID;
                switch (CompanyRoutingMode)
                {
                    case CompanyRM.Random:
                        var company = CompanyMoons[UnityEngine.Random.Range(0, CompanyMoons.Count)];
                        Logger.LogInfo($"Navigating to Company {company.PlanetName}");
                        levelID = company.levelID;
                        break;
                    case CompanyRM.Select:
                        if (SelectedCompanyID == -1)
                        {
                            foreach (var level in StartOfRound.Instance.levels)
                            {
                                if (SelectedCompanyName == Utils.GetNormalizedMoonName(level))
                                {
                                    SelectedCompanyID = level.levelID;
                                    break;
                                }
                            }
                        }
                        if (SelectedCompanyID != -1)
                        {
                            Logger.LogInfo("Force navigating to " + SelectedCompanyName);
                            levelID = SelectedCompanyID;
                        }
                        else
                        {
                            Logger.LogError("Selected Company Moon " + SelectedCompanyName + " was not found. Force navigating to the Company Building instead.");
                        }
                        break;
                    default:
                        return;
                }
                StartOfRound.Instance.ChangeLevelServerRpc(levelID, FindObjectOfType<Terminal>().groupCredits);
            }
            else
            {
                SelectableLevel selectableLevel = TimeOfDay.Instance.currentLevel;
                while (selectableLevel.PlanetName == TimeOfDay.Instance.currentLevel.PlanetName)
                {
                    if (config.ActivateMoonsDistributions.Value)
                        selectableLevel = Utils.GetARandomMoonByDistribution();
                    else
                        selectableLevel = StartOfRound.Instance.levels[UnityEngine.Random.Range(0, StartOfRound.Instance.levels.Length)];
                    if (!Utils.IsMoonValid(selectableLevel))
                    {
                        selectableLevel = TimeOfDay.Instance.currentLevel;
                    }
                }
                Logger.LogInfo($"Navigating to {selectableLevel.PlanetName}");
                StartOfRound.Instance.ChangeLevelServerRpc(selectableLevel.levelID, FindObjectOfType<Terminal>().groupCredits);
            }
            IsStarting = true;
        }

        public void StartRandomPlanet()
        {
            StartOfRound.Instance.StartGameServerRpc();
        }
    }
}
