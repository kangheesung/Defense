using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defense;

public class DummyFrame : MonoBehaviour {
    public ItemType FrameType;  //  프레임타입
    public ItemType AttachmentSlot; //  부착물일때 쓰는 슬롯
    public Attachments FrameAttachmentSlot; //  부착물 타입
    public weaponSlotType FrameSlotType;    //  슬롯 타입

    private bool active = false;    //  활성화 토글

    public void setActive (bool act) {
        active = act;
        if (transform.childCount > 1) { //  무기 슬롯
            if (active) transform.GetChild(0).gameObject.SetActive(true);
            else transform.GetChild(0).gameObject.SetActive(false);
        }
        else {  //  장비 슬롯
            if (active) gameObject.SetActive(true);
            else gameObject.SetActive(false);
        }
    }
}