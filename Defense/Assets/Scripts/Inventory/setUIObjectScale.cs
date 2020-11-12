using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defense;
public class setUIObjectScale : MonoBehaviour {
    Vector3 originalObjectScale;

    private void Awake() {
        originalObjectScale = transform.localScale; //  현재 오브젝트의 원래 크기
        setScaleScreen();   //  오브젝트 크기 재설정
    }

    public void setScaleScreen() {  //  현재 오브젝트 크기를 스크린에 맞춰 재설정
        transform.localScale = (Screen.height / 1080f) * originalObjectScale;
    }

    public void setScaleUi() {  //  현재 오브젝트 크기를 인벤토리 타입에 맞춰 재설정
        if (this.transform.parent.parent.GetComponent<Slot>() != null) {
            //  Slot
            ItemType tempItemType = this.transform.parent.parent.GetComponent<Slot>().inventoryType;
            switch (tempItemType) {
                case ItemType.InventorySlot:
                    transform.localScale = .5f * originalObjectScale;
                    break;
                case ItemType.Weapon:
                    transform.localScale = 1 * originalObjectScale;
                    break;
                case ItemType.Handgun:
                    transform.localScale = 1 * originalObjectScale;
                    break;
                case ItemType.Grenade:
                    transform.localScale = 1 * originalObjectScale;
                    break;
                default:    //  Equipment Slot
                    transform.localScale = .5f * originalObjectScale;
                    break;
            }
        } else {
            //  Ground Slot
            transform.localScale = 1 * originalObjectScale;
        }
    }
}
