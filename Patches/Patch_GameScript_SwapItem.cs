using GadgetCore.API;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using System;
using System.Reflection;

namespace GearSwap.Patches
{
    // this patch makes it so the particles from equipping gear don't show if the inventory is closed
    [HarmonyPatch()]
    [HarmonyGadget("GearSwap")]
    public static class Patch_GameScript_SwapItem
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            string GCPatchTypeName = "GadgetCore.Patches.Patch_GameScript_SwapItem, " + typeof(GadgetCore.GadgetLogger).Assembly.FullName;
            MethodInfo GCPatch = Type.GetType(GCPatchTypeName)?.GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static);
            if(GCPatch == null)
                GearSwap.logger.LogWarning("Failed to patch Prefix in " + GCPatchTypeName);
            else
                yield return GCPatch;
            yield return typeof(GameScript).GetMethod("SwapItem", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            // Find the first call to Instantiate after the clickBurst string is loaded, and replace it with our own method and return.
            for(int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "clickBurst") 
                {
                    for(; i < codes.Count; i++)
                    {
                        if(codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == "UnityEngine.Object Instantiate(UnityEngine.Object, Vector3, Quaternion)")
                        {
                            codes[i].operand = AccessTools.Method(typeof(Patch_GameScript_SwapItem), "InstantiateClickBurst");
                            foreach(var code in codes)
                                GearSwap.Log(code.ToString());
                            return codes;
                        }
                    }
                }
            }
            throw new Exception("Failed to patch equip particles.");
        }

        public static UnityEngine.Object InstantiateClickBurst(UnityEngine.Object prefab, Vector3 slotPos, Quaternion rotation)
        {
            if(InstanceTracker.GameScript.inventoryMain.activeInHierarchy)
                return GameObject.Instantiate(prefab, slotPos, rotation);
            else
                return null; // this would be a problem if something actually used the instantiated object
        }
    }
}
