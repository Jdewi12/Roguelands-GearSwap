using UnityEngine;
using GadgetCore.API;

namespace GearSwap
{
    [Gadget(nameof(GearSwap), RequiredOnClients: false)]
    public class GearSwap : Gadget 
    {
        public const string MOD_VERSION = "1.0";
        public const string CONFIG_VERSION = "1.0"; // Increment whenever config format changes.

        public static GadgetCore.GadgetLogger logger;

        public static void Log(string text)
        {
            logger.Log(text);
        }

        protected override void PrePatch()
        {
            logger = base.Logger;
        }

        protected override void Initialize()
        {
            Logger.Log(nameof(GearSwap) + " v" + Info.Mod.Version);

        }
    }
}