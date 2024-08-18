using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Dresser.Scripts
{
    class DresserUIScript : MonoBehaviour
    {
        private void OnEnable()
        {
            if (WeaponSwap.dresserInstance != gameObject)
            {
                WeaponSwap.menuDresser = transform.Find("menuDresser");
                WeaponSwap.menuStuffSelect = transform.Find("menuStuffSelect");
                WeaponSwap.mMenuStuffSelect = WeaponSwap.menuStuffSelect.GetComponent<MeshRenderer>();
                WeaponSwap.boxes = WeaponSwap.menuStuffSelect.Find("boxes");
                WeaponSwap.pageBack = WeaponSwap.boxes.Find("bSelectorPageBack")?.GetComponent<ButtonMenu>();
                WeaponSwap.pageForward = WeaponSwap.boxes.Find("bSelectorPageForward")?.GetComponent<ButtonMenu>();
                WeaponSwap.dresserInstance = gameObject;
                WeaponSwap.hover = WeaponSwap.boxes.Find("hover");
                WeaponSwap.hover.transform.position = new Vector3(-10000, 0, WeaponSwap.hover.transform.position.z);
                WeaponSwap.chosen = WeaponSwap.boxes.Find("chosen");
                WeaponSwap.name = WeaponSwap.menuStuffSelect.Find("NAME").GetComponent<TextMesh>();
                WeaponSwap.name2 = WeaponSwap.name.transform.GetChild(0).GetComponent<TextMesh>();
                WeaponSwap.unlock = WeaponSwap.menuStuffSelect.Find("UNLOCK").GetComponent<TextMesh>();
                WeaponSwap.unlock2 = WeaponSwap.unlock.transform.GetChild(0).GetComponent<TextMesh>();
                WeaponSwap.desc = WeaponSwap.menuStuffSelect.Find("DESC").GetComponent<TextMesh>();
                WeaponSwap.desc2 = WeaponSwap.desc.transform.GetChild(0).GetComponent<TextMesh>();
                //Dresser.menuTitle = Dresser.menuDresser.Find("TITLE").GetComponent<TextMesh>();
                //Dresser.menuTitle2 = Dresser.menuTitle.transform.GetChild(0).GetComponent<TextMesh>();
                WeaponSwap.menuAug = WeaponSwap.menuDresser.Find("AUG").GetChild(0).GetComponent<TextMesh>();
                WeaponSwap.menuAug2 = WeaponSwap.menuAug.transform.GetChild(0).GetComponent<TextMesh>();
                WeaponSwap.menuUnif = WeaponSwap.menuDresser.Find("UNIF").GetChild(0).GetComponent<TextMesh>();
                WeaponSwap.menuUnif2 = WeaponSwap.menuUnif.transform.GetChild(0).GetComponent<TextMesh>();
            }
            WeaponSwap.currentUI = WeaponSwap.CurrentUI.MAIN;
            WeaponSwap.menuDresser.gameObject.SetActive(true);
            WeaponSwap.menuStuffSelect.gameObject.SetActive(false);
            WeaponSwap.menuAug.text = "Augment: " + InstanceTracker.Menuu.GetAugmentName(Menuu.curAugment);
            WeaponSwap.menuAug2.text = WeaponSwap.menuAug.text;
            WeaponSwap.menuUnif.text = "Uniform: " + InstanceTracker.Menuu.GetUniformName(Menuu.curUniform);
            WeaponSwap.menuUnif2.text = WeaponSwap.menuUnif.text;
        }

        private void OnDisable()
        {
            WeaponSwap.currentUI = WeaponSwap.CurrentUI.CLOSED;
        }

        
    }
}
