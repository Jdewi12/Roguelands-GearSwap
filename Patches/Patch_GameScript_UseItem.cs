
using GadgetCore.API;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace GearSwap.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("UseItem")]
    [HarmonyGadget("GearSwap")]
    public static class Patch_GameScript_UseItem
    {
        static FieldInfo inventoryField = typeof(GameScript).GetField("inventory", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo swapItemMethod = typeof(GameScript).GetMethod("SwapItem", BindingFlags.NonPublic | BindingFlags.Instance);

        static int ringState = 0;

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, int slot)
        {
            int equipSlot;
            Item[] inventoryCopy = (Item[])inventoryField.GetValue(__instance);
            Item newWeap = inventoryCopy[slot];
            int newID = newWeap.id;
            var slotType = ItemRegistry.GetTypeByID(newID);
            switch(slotType)
            {
                case ItemType.WEAPON:
                    equipSlot = 36;
                    break;
                case ItemType.OFFHAND:
                    equipSlot = 37;
                    break;
                case ItemType.HELMET:
                    equipSlot = 38;
                    break;
                case ItemType.ARMOR:
                    equipSlot = 39;
                    break;
                case ItemType.RING:
                    equipSlot = 40 + ringState;
                    ringState = 1 - ringState; // alternate between 0 and 1 so you can swap again to put the ring in the other slot
                    break;
                default:
                    return;
            }
            swapItemMethod.Invoke(__instance, new object[] { slot }); // put slot into hold
            swapItemMethod.Invoke(__instance, new object[] { equipSlot }); // swap hold with equipped
            swapItemMethod.Invoke(__instance, new object[] { slot }); // put old weapon back in slot
        }
    }
}
