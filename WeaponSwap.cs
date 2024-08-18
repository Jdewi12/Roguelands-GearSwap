using UnityEngine;
using GadgetCore.API;
using System.Collections.Generic;
using System.Collections;
using Dresser.Scripts;
using ScrapYard.API;
using System.Reflection;
using GadgetCore.Util;

namespace Dresser
{
    [Gadget(nameof(WeaponSwap), RequiredOnClients: false)]
    public class WeaponSwap : Gadget 
    {
        public const string MOD_VERSION = "1.0";
        public const string CONFIG_VERSION = "1.0"; // Increment whenever config format changes.

        public static GadgetCore.GadgetLogger logger;

        public AssetBundle assetBundle;

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
            Logger.Log(nameof(WeaponSwap) + " v" + Info.Mod.Version);

        }
    }
}