using UnityEngine;
using System.Collections.Generic;
using Defense;

public class ItemDatabase : MonoBehaviour {
    public static ItemDatabase instance;
    public List<Item> items = new List<Item>(); //  아이템 리스트
    public int itemlength;  //  아이템 길이 (장비 등을 제외한 ItemDatabase의 아이템 마지막 값)

    private void Awake() {
        instance = this;

        string name;
        string desc;
        ItemType type;
        Attachments attachment;
        int maxCount;
        GameObject prefab;
        GameObject uiPrefab;
        for (int i = 0; i < this.transform.GetChild(0).childCount; i++) {   //  ItemDatabase의 자식 아이템들 리스트에 추가 
            name = this.transform.GetChild(0).GetChild(i).GetComponent<PlayerItem>().itemName;
            desc = this.transform.GetChild(0).GetChild(i).GetComponent<PlayerItem>().itemDesc;
            type = this.transform.GetChild(0).GetChild(i).GetComponent<PlayerItem>().itemType;
            attachment = this.transform.GetChild(0).GetChild(i).GetComponent<PlayerItem>().attachments;
            maxCount = this.transform.GetChild(0).GetChild(i).GetComponent<PlayerItem>().itemMaxCount;
            prefab = this.transform.GetChild(0).GetChild(i).GetComponent<PlayerItem>().itemPrefab;
            uiPrefab = this.transform.GetChild(0).GetChild(i).GetComponent<PlayerItem>().itemUIPrefab;
            Add(name, 1, items.Count, desc, type, attachment, maxCount, prefab, uiPrefab);  //  아이템 리스트에 추가
            this.transform.GetChild(0).GetChild(i).GetComponent<PlayerItem>().code = i; //  코드 설정
        }
        itemlength = items.Count;
    }
    //  Equipment의 경우 itemMaxCount = 1
    public void Add(string itemName, int itemValue, int code, string itemDesc, ItemType itemType, Attachments itemAttachments, int itemMaxCount, GameObject itemPrefab, GameObject itemUIPrefab) {
        items.Add(new Item(itemName, itemValue, code, itemDesc, itemType, itemAttachments, itemMaxCount, 0, itemPrefab, itemUIPrefab));
    }
}