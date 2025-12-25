using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace RandomMoonFX
{
    internal class Config
    {
        public readonly List<string> MoonsBlacklist = new List<string>();
        public readonly Dictionary<string, int> MoonsDistribution = new Dictionary<string, int>();
        public readonly ConfigEntry<bool> CelestialTintAnimation;
        public readonly ConfigEntry<bool> ChameleonAnimation;
        public readonly ConfigEntry<float> AnimationTimeOverride;
        public readonly ConfigEntry<bool> ActivateRandomMoons;
        public readonly ConfigEntry<bool> ExcludePreviouslyVisited;
        public readonly ConfigEntry<bool> ConstellationsCheck;
        public readonly ConfigEntry<string> MoonsBlacklistStr;
        public readonly ConfigEntry<bool> QuotaCheck;
        public readonly ConfigEntry<bool> RandomizeLastDay;
        public readonly ConfigEntry<string> CompanyRoutingMode;
        public readonly ConfigEntry<string> SelectedCompanyName;
        public readonly ConfigEntry<bool> ActivateFreeMoons;
        public readonly ConfigEntry<bool> ActivateMoonsDistributions;
        public readonly ConfigEntry<string> MoonsDistributionStr;

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;

            CelestialTintAnimation = cfg.Bind("Routing animation", "Celestial_Tint animation", true, "Enable compatibility with Celestial_Tint routing animation (which is a little bit longer than vanilla).\nWill be automatically false if Celestial_Tint is not installed.");
            ChameleonAnimation = cfg.Bind("Routing animation", "Chameleon animation", true, "Enable compatibility with Chameleon routing animation (which is longer than vanilla).\nWill be automatically false if Chameleon is not installed.");
            AnimationTimeOverride = cfg.Bind("Routing animation", "Animation time override", -1f, "If not -1, will change the time of the routing animation.\nIf you have Celestial_Tint or Chameleon compatibility enabled the time will be changed automatically but you can still modify it with this config.\nDefault value is 1.5, Celestial_Tint is 4 and Chameleon is 7.5.");

            ActivateRandomMoons = cfg.Bind("Randomization method", "Activate Random Moons", true, "Allow the mod to randomize the moon. Disabling this config effectively prevent the mod to work.\nFor testing purpose.");
            ExcludePreviouslyVisited = cfg.Bind("Randomization method", "Exclude previously visited", false, "Enable this if you want to exclude already visited moons from the randomization method. This will reset when all moons have been seen once.");
            ConstellationsCheck = cfg.Bind("Randomization method", "LethalConstellations check", true, "Enable compatibility with LethalConstellations by preventing the randomization method to choose a moon that is not included in the current constellations.\nWill be automatically false if LethalConstellations is not installed.");
            MoonsBlacklistStr = cfg.Bind("Randomization method", "Moons Blacklist", "", "Comma separated list of moons that will never be chosen by the randomization method, if you don't want to play on specifics moons.\nThis Experimentation,Embrion,Asteroid-14,Espira is a valid example.");

            QuotaCheck = cfg.Bind("Last day check", "Quota check", true, "If true, the ship will route to the Company on the last day if quota has not been met yet. If false, there will be no quota check and the ship will route to the Company only on the last day.");
            RandomizeLastDay = cfg.Bind("Last day check", "Randomize last day", false, "Enable this if you don't want to auto route to the Company on the last day if there is not enough scraps in the ship to meet quota (other players potential dead bodies included), this allows you to randomize a moon on the last day to mess around but you will be fired at the end of the day.");
            CompanyRoutingMode = cfg.Bind("Last day check", "Company routing mode", "Random", new ConfigDescription("Choose how the mod works when auto-routing on the Company on the last day.\n'Random' will select a random Company moon (vanilla or modded).\n'Select' will route to the moon defined by the 'Selected Company' config.\n'Manual' will disable auto routing to the Company so players must route manually on the last day.", new AcceptableValueList<string>("Random", "Select", "Manual")));
            SelectedCompanyName = cfg.Bind("Last day check", "Selected Company", "Gordion", "When 'Company routing mode' is set to 'Select', you can use this config to choose which moon is going to be auto-routed to on the last day.\nIf the moon is not found, it will default to Gordion.");

            ActivateFreeMoons = cfg.Bind("Misc", "Activate Free Moons", true, "Prevent credits consumption when manually routing to a moon in the terminal. This is for avoiding people wasting money on a moon that is not going to be played because of the randomization method.\nThis config works even if 'Activate Random Moons' is false.");

            ActivateMoonsDistributions = cfg.Bind("Moons Distribution", "Activate Moons Distributions", false, "If enabled Moons Distribution list will work as white list.");
            MoonsDistributionStr = cfg.Bind("Moons Distribution", "Moons Distribution", "Vow=1,Adamance=1,Rend=3,Dine=1,Titan=1,Artifice=5,Embrion=1", "You need to input list of moons with their distribution value.\nDefault value is 1, you can input whatever integer number you want, it will calculate distribution based on all moons value.\nExample of input:\nRend=1,Dine=1,Titan=1,Artifice=2");

            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupCustomConfigs()
        {
            if (MoonsBlacklistStr != null && MoonsBlacklistStr.Value != "")
            {
                foreach (string moonName in MoonsBlacklistStr.Value.Split(',').Select(s => s.Trim()))
                {
                    MoonsBlacklist.Add(Utils.GetNormalizedMoonName(moonName));
                }
            }
            if (MoonsDistributionStr != null && MoonsDistributionStr.Value != "")
            {
                foreach (string moon in MoonsDistributionStr.Value.Split(',').Select(s => s.Trim()))
                {
                    var moonName = Utils.GetNormalizedMoonName(moon.Split('=')[0]);
                    var moonChanceStr = moon.Split('=')[1];
                    if (int.TryParse(moonChanceStr, out int moonChance))
                        MoonsDistribution.Add(moonName, moonChance);
                    else
                        MoonsDistribution.Add(moonName, 0);
                }
            }
            
        }
    }
}
