using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defense;

public class PlayerEquipment : MonoBehaviour {
    //  캐릭터 Equipment에 사용(head, body, legs 등)
    public int equipmentCode;
    public string equipmentName = "Armor";
    public string equipmentDesc = "";
    public ItemType equipmentType = ItemType.Chest;
    public int equipmentMaxCount = 1;
    public GameObject equipmentPrefab;
    public GameObject equipmentUIPrefab;
}