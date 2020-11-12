using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defense;

public class PlayerItem : MonoBehaviour {
    //  ItemDatabase에 자식 아이템에 사용
    public string itemName = "Item";
    public string itemDesc = "";
    public ItemType itemType = ItemType.Consumption;
    public Attachments attachments;
    public int itemMaxCount = 1;
    public GameObject itemPrefab;
    public GameObject itemUIPrefab;

    public int code;
}
