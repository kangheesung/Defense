using UnityEngine;
using Defense;

[System.Serializable]
public class Item {
    public string itemName; //  아이템 이름
    public int itemValue;   //  아이템 가치(존재 유무) 0 : 없음, 1 : 있음
    public int itemCode;    //  아이템 코드
    public string itemDesc; //  아이템 설명
    public ItemType itemType;   //  아이템 타입(종류)
    public Attachments itemAttachment;  //  아이템 부착물 (타입이 부착물인 경우에)
    public int itemMaxCount;    //  아이템 최대 개수
    public int itemCount;   //  아이템 개수
    public GameObject itemPrefab;   //  아이템 프리팹
    public GameObject itemUIPrefab; //  아이템 UI프리팹

    //  아이템 설정
    public Item(string _itemName, int _itemValue, int _itemCode, string _itemDesc, ItemType _itemType, Attachments _itemAttachment, int _itemMaxCount , int _itemCount, GameObject _itemPrefab, GameObject _itemUIPrefab) {
        itemName = _itemName;
        itemValue = _itemValue;
        itemCode = _itemCode;
        itemDesc = _itemDesc;
        itemType = _itemType;
        itemAttachment = _itemAttachment;
        itemMaxCount = _itemMaxCount;
        itemCount = _itemCount;
        itemPrefab = _itemPrefab;
        itemUIPrefab = _itemUIPrefab;
    }

    //  아이템 설정(Copy)
    public Item(Item _item) {
        itemName = _item.itemName;
        itemValue = _item.itemValue;
        itemCode = _item.itemCode;
        itemDesc = _item.itemDesc;
        itemType = _item.itemType;
        itemAttachment = _item.itemAttachment;
        itemMaxCount = _item.itemMaxCount;
        itemCount = _item.itemCount;
        itemPrefab = _item.itemPrefab;
        itemUIPrefab = _item.itemUIPrefab;
    }

    //  아이템 리셋
    public void resetItem() {
        itemName = "";
        itemValue = 0;
        itemCode = -1;
        itemDesc = "";
        itemType = ItemType.InventorySlot;
        itemAttachment = Attachments.LowerMount;
        itemMaxCount = 0;
        itemCount = 0;
        itemPrefab = null;
        itemUIPrefab = null;
    }
}