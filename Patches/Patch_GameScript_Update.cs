using GadgetCore;
using GadgetCore.API;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Dresser.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Update")]
    [HarmonyGadget("Dresser")]
    public static class Patch_GameScript_Update
    {
        static int hovering = -1;

        static FieldInfo stuffSelecting = typeof(Menuu).GetField("stuffSelecting", BindingFlags.NonPublic | BindingFlags.Instance);

        const float leftSlotX = -11;
        const float topSlotY = 6.875f;
        const float slotSize = 2;
        const int slotsPerRow = 12;

        [HarmonyPrefix]
        public static void Prefix()
        {
            if(WeaponSwap.currentUI != WeaponSwap.CurrentUI.CLOSED)
            {
                if(GameScript.inventoryOpen && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit, 100))
                {
                    GameObject hit = raycastHit.transform.gameObject;
                    if(hit.layer == 22) // stuffSelect
                    {
                        if(int.TryParse(hit.name, out int slot)) // hovering slot
                        {
                            if(hovering != slot)
                            {
                                MoveHover(slot);
                                SoundHover();
                            }
                            if(Input.GetMouseButtonDown(0)) // clicked slot
                            {
                                SoundConfirm();
                                ICharacterFeatureRegistry characterFeatureRegistry;
                                if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                                    characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(1);
                                else if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.UNIF)
                                    characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(2);
                                else
                                    return;

                                int id;
                                if(characterFeatureRegistry.GetCurrentPage() == 1)
                                {
                                    id = slot;
                                }
                                else
                                {
                                    id = slot + characterFeatureRegistry.GetCurrentPage() * 24;
                                }

                                if(characterFeatureRegistry.TryGetEntryInterface(id, out _))
                                {
                                    if(characterFeatureRegistry.IsFeatureUnlocked(id))
                                    {
                                        if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                                            Menuu.curAugment = id;
                                        else if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.UNIF)
                                            Menuu.curUniform = id;
                                        MoveChosen(id);
                                        ChangeChar();
                                    }
                                }
                                else if(characterFeatureRegistry.GetCurrentPage() == 1)
                                {
                                    if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                                    {
                                        if(Menuu.unlockedAugment[slot] != 0)
                                        {
                                            Menuu.curAugment = slot;
                                            MoveChosen(slot);
                                            ChangeChar();
                                        }
                                    }
                                    else if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.UNIF)
                                    {
                                        if(Menuu.unlockedUniform[slot] != 0)
                                        {
                                            Menuu.curUniform = slot;
                                            MoveChosen(slot);
                                            ChangeChar();
                                        }
                                    }
                                }
                            }
                        }
                        else // somehow hovering stuff select but not any slots (not sure if possible)
                        {
                            UnHover();
                        }
                    }
                    else // not hovering stuff select
                    {
                        UnHover();
                        if(Input.GetMouseButtonDown(0))
                        {
                            switch(hit.name)
                            {
                                case "AUG":
                                    SoundConfirm();
                                    WeaponSwap.menuDresser.gameObject.SetActive(false);
                                    WeaponSwap.menuStuffSelect.gameObject.SetActive(true);
                                    WeaponSwap.currentUI = WeaponSwap.CurrentUI.AUG;
                                    stuffSelecting.SetValue(InstanceTracker.Menuu, 1);
                                    if(Menuu.curAugment < 24)
                                        CharacterAugmentRegistry.CurrentPage = 1;
                                    else
                                        CharacterAugmentRegistry.CurrentPage = Menuu.curAugment / 24;
                                    MoveChosen(Menuu.curAugment); // move chosen to the current augment
                                    SetDescription(Menuu.curAugment);
                                    UpdateBoxes();
                                    break;
                                case "UNIF":
                                    SoundConfirm();
                                    WeaponSwap.menuDresser.gameObject.SetActive(false);
                                    WeaponSwap.menuStuffSelect.gameObject.SetActive(true);
                                    WeaponSwap.currentUI = WeaponSwap.CurrentUI.UNIF;
                                    stuffSelecting.SetValue(InstanceTracker.Menuu, 2);
                                    if(Menuu.curUniform < 24)
                                        CharacterUniformRegistry.CurrentPage = 1;
                                    else
                                        CharacterUniformRegistry.CurrentPage = Menuu.curUniform / 24;
                                    MoveChosen(Menuu.curUniform); // move chosen to the current uniform
                                    SetDescription(Menuu.curUniform);
                                    UpdateBoxes();
                                    break;
                                case "bSelectorPageBack":
                                    SoundConfirm();
                                    if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                                    {
                                        if(CharacterAugmentRegistry.CurrentPage > 1)
                                        {
                                            CharacterAugmentRegistry.CurrentPage -= 1;
                                            MoveChosen(Menuu.curAugment);
                                        }
                                    }
                                    else if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.UNIF)
                                    {
                                        if(CharacterUniformRegistry.CurrentPage > 1)
                                        {
                                            CharacterUniformRegistry.CurrentPage -= 1;
                                            MoveChosen(Menuu.curUniform);
                                        }
                                    }
                                    UpdateBoxes();
                                    break;
                                case "bSelectorPageForward":
                                    SoundConfirm();
                                    if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                                    {
                                        if((CharacterAugmentRegistry.CurrentPage - 1) * 24 < CharacterAugmentRegistry.Singleton.Count())
                                        {
                                            CharacterAugmentRegistry.CurrentPage += 1;
                                            MoveChosen(Menuu.curAugment);
                                        }
                                    }
                                    else if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.UNIF)
                                    {
                                        if((CharacterUniformRegistry.CurrentPage - 1) * 24 < CharacterUniformRegistry.Singleton.Count())
                                        {
                                            CharacterUniformRegistry.CurrentPage += 1;
                                            MoveChosen(Menuu.curUniform);
                                        }
                                    }
                                    UpdateBoxes();
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
        static void SoundConfirm()
        {
            var a = InstanceTracker.GameScript.GetComponent<AudioSource>();
            if(a != null)
                a.PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
        }
        static void SoundHover()
        {
            var a = InstanceTracker.GameScript.GetComponent<AudioSource>();
            if(a != null)
                a.PlayOneShot((AudioClip)Resources.Load("Au/hover"), Menuu.soundLevel / 10f);
        }
        static void MoveHover(int slot)
        {
            if(hovering != slot)
            {
                hovering = slot;

                Vector2 slotPos = GetSlotPosition(slot);
                WeaponSwap.hover.transform.localPosition = new Vector3(slotPos.x, slotPos.y, WeaponSwap.hover.transform.localPosition.z);

                ICharacterFeatureRegistry characterFeatureRegistry;
                if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                    characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(1);
                else if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.UNIF)
                    characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(2);
                else
                    return;

                int id;
                if(characterFeatureRegistry.GetCurrentPage() == 1)
                    id = slot;
                else
                    id = slot + characterFeatureRegistry.GetCurrentPage() * 24;
                SetDescription(id);
            }
        }
        static void MoveChosen(int id)
        {
            ICharacterFeatureRegistry characterFeatureRegistry;
            if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(1);
            else if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.UNIF)
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(2);
            else
                return;
            // if same page
            if((characterFeatureRegistry.GetCurrentPage() == 1 && id < 24) ||
                (id / 24 == characterFeatureRegistry.GetCurrentPage()))
            {
                int slot = id % 24;
                Vector2 slotPos = GetSlotPosition(slot);

                WeaponSwap.chosen.transform.localPosition = new Vector3(slotPos.x, slotPos.y, WeaponSwap.chosen.transform.localPosition.z);
            }
            else // if different page
            {
                WeaponSwap.chosen.transform.localPosition = new Vector3(-10000, 0, WeaponSwap.chosen.transform.localPosition.z);
            }
        }

        static Vector2 GetSlotPosition(int slot)
        {
            return new Vector2(
                leftSlotX + (slot % slotsPerRow) * slotSize,
                topSlotY - (slot / slotsPerRow /*integer division*/) * slotSize
            );
        }

        static void UnHover()
        {
            if(hovering != -1)
            {
                hovering = -1;
                WeaponSwap.hover.transform.position = new Vector3(-10000, 0, WeaponSwap.hover.transform.position.z);
                SetDescription((WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG) ? Menuu.curAugment : Menuu.curUniform);
            }
        }

        static void SetDescription(int id)
        {
            string name = "";
            string desc = "";
            string unlock = "";
            ICharacterFeatureRegistry characterFeatureRegistry;
            if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(1);
            else if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.UNIF)
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(2);
            else
                return;

            if(characterFeatureRegistry.TryGetEntryInterface(id, out ICharacterFeatureRegistryEntry entry))
            {
                if(characterFeatureRegistry.IsFeatureUnlocked(id))
                {
                    name = entry.GetName();
                    desc = entry.GetDesc();
                    unlock = entry.GetUnlockCondition();
                }
                else
                {
                    name = entry.GetLockedName();
                    desc = entry.GetLockedDesc();
                    unlock = entry.GetLockedUnlockCondition();
                }
            }
            else if(id < 24) // vanilla
            {
                if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                {
                    if(Menuu.unlockedAugment[id] != 0)
                    {
                        name = InstanceTracker.Menuu.GetAugmentName(id);
                        desc = InstanceTracker.Menuu.GetAugmentDesc(id);
                    }
                    unlock = InstanceTracker.Menuu.GetAugmentUnlock(id);
                }
                else if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.UNIF)
                {
                    if(Menuu.unlockedUniform[id] != 0)
                    {
                        name = InstanceTracker.Menuu.GetUniformName(id);
                        desc = InstanceTracker.Menuu.GetUniformDesc(id);
                    }
                    unlock = InstanceTracker.Menuu.GetUniformUnlock(id);
                }
            }
            else
            {
                int equippedIndex = (WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG) ? Menuu.curAugment : Menuu.curUniform;
                if(equippedIndex != id)
                {
                    SetDescription(equippedIndex);
                }
                else
                {
                    string currentUIName = (WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG) ? "augment" : "uniform";
                    WeaponSwap.logger.LogConsole("Unknown currently-equipped " + currentUIName + " with id: " + id, GadgetConsole.MessageSeverity.WARN);
                }
                return;
            }
            WeaponSwap.name.text = name;
            WeaponSwap.name2.text = name;
            WeaponSwap.desc.text = desc;
            WeaponSwap.desc2.text = desc;
            WeaponSwap.unlock.text = unlock;
            WeaponSwap.unlock2.text = unlock;
        }

        static void UpdateBoxes()
        {
            ICharacterFeatureRegistry characterFeatureRegistry;
            if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
            {
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(1);
                if(CharacterAugmentRegistry.Singleton.Count() == 0)
                {
                    WeaponSwap.pageBack.gameObject.SetActive(false);
                    WeaponSwap.pageForward.gameObject.SetActive(false);
                }
                else
                {
                    WeaponSwap.pageBack.gameObject.SetActive(true);
                    WeaponSwap.pageForward.gameObject.SetActive(true);
                }
            }
            else if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.UNIF)
            {
                characterFeatureRegistry = PatchMethods.GetCharacterFeatureRegistry(2);
                if(CharacterUniformRegistry.Singleton.Count() == 0)
                {
                    WeaponSwap.pageBack.gameObject.SetActive(false);
                    WeaponSwap.pageForward.gameObject.SetActive(false);
                }
                else
                {
                    WeaponSwap.pageBack.gameObject.SetActive(true);
                    WeaponSwap.pageForward.gameObject.SetActive(true);
                }
            }
            else
            {
                return;
            }

            if(characterFeatureRegistry.GetCurrentPage() == 1)
            {
                if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                    WeaponSwap.mMenuStuffSelect.material = (Material)Resources.Load("mat/mUpgrade");
                else // UNIF
                    WeaponSwap.mMenuStuffSelect.material = (Material)Resources.Load("mat/mUniform");
            }
            else
            {
                if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                    WeaponSwap.mMenuStuffSelect.material = (Material)Resources.Load("mat/mAugmentBack");
                else // UNIF
                    WeaponSwap.mMenuStuffSelect.material = (Material)Resources.Load("mat/mUniformBack");
            }


            for(int i = 0; i < WeaponSwap.boxes.childCount; i++)
            {
                Transform box = WeaponSwap.boxes.GetChild(i);
                if(int.TryParse(box.name, out int slot))
                {
                    box.gameObject.SetActive(true);
                    Transform icon = box.GetChild(0);
                    Transform background = icon.GetChild(0);

                    int id;
                    if(characterFeatureRegistry.GetCurrentPage() == 1)
                        id = slot;
                    else
                        id = slot + characterFeatureRegistry.GetCurrentPage() * 24;

                    if(characterFeatureRegistry.TryGetEntryInterface(id, out _))
                    {
                        Material mat;
                        if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                            mat = CharacterAugmentRegistry.Singleton.GetEntry(id).SelectorIconMat;
                        else // UNIF
                            mat = CharacterUniformRegistry.Singleton.GetEntry(id).SelectorIconMat;
                        icon.GetComponent<Renderer>().material = mat;
                        icon.GetComponent<Renderer>().enabled = true;
                        background.GetComponent<Renderer>().enabled = true;
                        box.GetComponent<Renderer>().enabled = !characterFeatureRegistry.IsFeatureUnlocked(id);
                    }
                    else if(characterFeatureRegistry.GetCurrentPage() == 1) // vanilla
                    {
                        bool unlocked;
                        if(WeaponSwap.currentUI == WeaponSwap.CurrentUI.AUG)
                            unlocked = Menuu.unlockedAugment[id] != 0;
                        else // UNIF)
                            unlocked = Menuu.unlockedUniform[id] != 0;
                        icon.GetComponent<Renderer>().enabled = false;
                        background.GetComponent<Renderer>().enabled = false;
                        box.GetComponent<Renderer>().enabled = !unlocked;
                    }
                    else
                    {
                        box.gameObject.SetActive(false);
                    }
                }
            }
        }

        static void ChangeChar()
        {
            PreviewLabs.PlayerPrefs.SetInt(Menuu.curChar + "uniform", Menuu.curUniform);
            PreviewLabs.PlayerPrefs.SetInt(Menuu.curChar + "augment", Menuu.curAugment);

            GameScript.equippedIDs[6] = Menuu.curUniform;
            GameScript.equippedIDs[7] = Menuu.curAugment;
            MenuScript.playerAppearance.GetComponent<PlayerAppearance>().UA(GameScript.equippedIDs, 0, GameScript.dead);
            MenuScript.playerAppearance.GetComponent<NetworkView>().RPC("UA", RPCMode.AllBuffered, new object[]
            {
                GameScript.equippedIDs,
                0,
                GameScript.dead
            });
            InstanceTracker.GameScript.UpdateHP();
            InstanceTracker.GameScript.UpdateMana();
            WeaponSwap.menu.CloseMenu();
        }
    }
}
