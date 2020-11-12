using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Defense;

public class Inventory : MonoBehaviour {
    public Camera invCam;   //  인벤토리를 표시할 카메라
    public static Inventory instance;
    public int inventoryX = 4, inventoryY = 4;  //  인벤토리 크기

    [HideInInspector]
    public int inventoryCount;  //  인벤토리 개수(inventroyType으로만)
    [HideInInspector]
    public bool GroundItemSlot; //  Ground슬롯 확인
    [HideInInspector]
    public bool EquipmentSlotFull;  //  인벤토리 꽉참
    [HideInInspector]
    public Slot itemPickUpTempSlot; //  픽업 아이템 임시 변수

    public Transform slot;  //  슬롯 프리팹
    public Transform draggingItem;  //  드래그 아이템 프리팹
    public List<Slot> slotScripts = new List<Slot>();   //  슬롯 리스트

    public List<Item> itemInventory = new List<Item>(); //  현재 소지중인 아이템 리스트

    public Slot enteredSlot;    //  마우스 커서가 올라가 있는 슬롯을 저장

    public List<Transform> equipmentSlots = new List<Transform>();  //  장비 슬롯

    public List<DummyFrame> dummyframeslots = new List<DummyFrame>();   //  더미 아이콘 슬롯

    public GameObject dummyPrefab;  //  더미 아이템

    private PlayerInteraction pi;
    private EquipmentManager em;

    void Awake() {
        instance = this;
        pi = FindObjectOfType<PlayerInteraction>();
        em = FindObjectOfType<EquipmentManager>();
    }
    
    void Start() {
        SlotMake(inventoryX, inventoryY, 0.08f);    //  인벤토리 슬롯 생성 (X, Y, 간격)
        AddItem(1, 1);  //  아이템 추가(1번 아이템 1개)
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            AddItem(1, 1);
        }
        if (enteredSlot != null) {
            if (enteredSlot.item.itemValue == 1) {
                if (Input.GetKeyDown(KeyCode.Mouse1)) { //  우클릭
                    playerInteractionDropItem(enteredSlot); //  해당 슬롯 아이템 드롭
                }
            }
        }
    }

    private void SlotMake(int xCount, int yCount, float xInterval) {    //  인벤토리 슬롯 생성 (X, Y, 간격)
        //  인벤토리 슬롯 생성
        Vector2 panelSize = new Vector2(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height);   //  인벤토리 rect w h 크기 벡터
        float xWidthRate = (1 - xInterval * (xCount + 1)) / xCount;
        float yWidthRate = panelSize.x * xWidthRate / panelSize.y;
        float yInterval = (1 - yWidthRate * yCount) / (yCount + 1);
        for (int y = 0; y < yCount; y++) {
            for (int x = 0; x < xCount; x++) {
                Transform newSlot = Instantiate(slot);
                newSlot.name = "Slot" + (y  + 1) + "." + (x + 1);
                newSlot.SetParent(transform);
                RectTransform slotRect = newSlot.GetComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(xWidthRate * x + xInterval * (x + 1), 1 - (yWidthRate * (y + 1) + yInterval * (y + 1)));
                slotRect.anchorMax = new Vector2(xWidthRate * (x + 1) + xInterval * (x + 1), 1 - (yWidthRate * y + yInterval * (y + 1)));
                slotRect.offsetMin = Vector2.zero;
                slotRect.offsetMax = Vector2.zero;
                slotScripts.Add(newSlot.GetComponent<Slot>());
                newSlot.GetComponent<Slot>().number = y * xCount + x;

                newSlot.localPosition = new Vector3(newSlot.localPosition.x, newSlot.localPosition.y, 0);
                newSlot.localScale = new Vector3(1, 1, 1);
            }
        }

        inventoryCount = slotScripts.Count; //  인벤토리 슬롯 수

        //  장비슬롯 number 설정
        for (int i = 0; i < equipmentSlots.Count; i++) {
            equipmentSlots[i].GetChild(0).GetComponent<Slot>().number = i + inventoryCount;
        }

        //  장비슬롯 슬롯 리스트에 추가
        for (int i = 0; i < equipmentSlots.Count; i++) {
            slotScripts.Add(equipmentSlots[i].GetChild(0).GetComponent<Slot>());
        }

        slotRestrict(xCount, yCount, inventoryY - 1, true); //  슬롯 제한 (X크기, Y크기, 적용할 줄, 제한)
        slotRestrict(xCount, yCount, inventoryY - 2, true);
        slotRestrict(xCount, yCount, inventoryY - 3, true);
    }

    public void slotRestrict(int xCount, int yCount, int restrictY, bool restrict) {    //  슬롯 제한(X크기, Y크기, 적용할 줄, 제한)
        for (int i = 0; i < xCount; i++) {
            int restrictSlot = restrictY * xCount + i;
            if (restrict) {
                slotScripts[restrictSlot].transform.GetChild(2).gameObject.SetActive(true);
                slotScripts[restrictSlot].inventoryType = ItemType.RestrictSlot;
            } else {
                slotScripts[restrictSlot].transform.GetChild(2).gameObject.SetActive(false);
                slotScripts[restrictSlot].inventoryType = ItemType.InventorySlot;
            }
            //  제한하려는 슬롯에 아이템이 있다면
            if (slotScripts[restrictSlot].item.itemValue == 1) {
                for (int j = 0; j < inventoryCount; j++) {
                    //  빈 슬롯 탐색
                    if (slotScripts[j].item.itemValue == 0 && slotScripts[j].inventoryType != ItemType.RestrictSlot) {
                        //  빈 슬롯으로 아이템 이동
                        Item tempItem = slotScripts[restrictSlot].item;
                        slotScripts[restrictSlot].item = slotScripts[j].item;
                        slotScripts[j].item = tempItem;
                        ItemImageChange(slotScripts[restrictSlot], false);
                        ItemImageChange(slotScripts[j], false);
                        break;
                    }
                    if (slotScripts[j].inventoryType == ItemType.RestrictSlot) {
                        //  빈 슬롯이 없다면 아이템을 드롭
                        pi.dropitem(slotScripts[restrictSlot], true);
                        break;
                    }
                }
            }
        }
    }

    public virtual void AddItem(int number, int itemCount) {    //  아이템 추가 (코드, 아이템 코드)
        int sameItemSlotNumber = -1;    //  추가하려는 아이템이 인벤토리에 없다면 -1 있다면 해당 슬롯 번호
        EquipmentSlotFull = false;  //  인벤토리 여유 슬롯 여부 
        //  추가하려는 아이템과 같은 아이템 슬롯 번호 설정
        for (int i = 0; i < inventoryCount; i++) {
            //  추가하려는 아이템과 이름이 같고 maxCount를 넘지 않으면
            if (slotScripts[i].item.itemName == ItemDatabase.instance.items[number].itemName && slotScripts[i].item.itemCount + itemCount <= ItemDatabase.instance.items[number].itemMaxCount) {
                sameItemSlotNumber = i; //  추가하려는 슬롯 번호를 해당 번호로 설정
                break;
            }
        }
        if (sameItemSlotNumber == -1) { //  인벤토리에 같은 아이템이 없다면
            for (int i = 0; i < slotScripts.Count; i++) {
                //  해당 슬롯의 인벤토리 타입이 추가하려는 아이템과 같거나 인벤토리 슬롯이면서 추가하려는 아이템 타입이 컨슘이면 
                if (slotScripts[i].inventoryType == ItemDatabase.instance.items[number].itemType ||
                    (slotScripts[i].inventoryType == ItemType.InventorySlot && ItemDatabase.instance.items[number].itemType == ItemType.Consumption)
                    ) {
                    //  해당 슬롯의 아이템 값이 0이면(비었다면)
                    if (slotScripts[i].item.itemValue == 0) {
                        //  해당 슬롯의 아이템 타입이 부착물이면
                        if (ItemDatabase.instance.items[number].itemType == ItemType.Attachment) {
                            //  해당 슬롯의 부착물 타입이 추가하려는 아이템과 다르다면 continue
                            if (slotScripts[i].attachmentSlot != ItemDatabase.instance.items[number].itemAttachment) {
                                continue;
                            }
                            //  해당 슬롯의 부착물 부모 슬롯이 null이 아니고
                            if (slotScripts[i].attachmentParentSlot != null) {
                                //  해당 슬롯의 부착물 부모 슬롯의 아이템의 아이템 값이 0이면 continue (무기가 없는데 부착물이 추가되는 것을 예방)
                                if (slotScripts[i].attachmentParentSlot.item.itemValue == 0) {
                                    continue;
                                }
                            }
                        }
                        Item item = ItemDatabase.instance.items[number];
                        itemInventory.Add(new Item(item));  //  소지중인 아이템 리스트에 추가할 아이템 추가
                        itemInventory[itemInventory.Count - 1].itemCount += itemCount;  //  방금 추가한 아이템에 아이템 개수 추가
                        slotScripts[i].item = itemInventory[itemInventory.Count - 1];   //  해당 슬롯에 추가한 아이템 적용
                        ItemImageChange(slotScripts[i], false); //  아이템 이미지 설정 (해당 슬롯, 더미 이미지 활성화 유무)
                        itemPickUpTempSlot = slotScripts[i];    //  픽업 아이템 임시 변수
                        return;
                    }
                }
            }
            EquipmentSlotFull = true;   //  인벤토리 꽉참
        } else {    //  인벤토리에 같은 아이템이 있다면
            slotScripts[sameItemSlotNumber].item.itemCount += itemCount;    //  해당 아이템에 개수 추가
            slotScripts[sameItemSlotNumber].transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = "" + slotScripts[sameItemSlotNumber].item.itemCount;  //  개수 표시
        }
    }

    public void removeItem(Slot _slot) {
        itemInventory.RemoveAll(x => x == _slot.item);  //  소지 아이템 리스트에서 해당 슬롯 아이템을 제거(해당 아이템만)
        _slot.item.resetItem(); //  아이템 슬롯 리셋
        _slot.transform.GetChild(0).gameObject.SetActive(false);    //  아이템 이미지 비활성화
        if (_slot.inventoryType == ItemType.InventorySlot) {
            _slot.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);    //  인벤토리 타입이면 아이템 개수도 비활성화
        }
        Destroy(_slot.transform.GetChild(0).GetChild(0).gameObject);    //  아이템 모델 제거
        if (_slot.inventoryType != ItemType.InventorySlot) {
            DummyImageChange(_slot, true);  //  인벤토리 타입이 아니라면 더미 이미지 활성화
        }
    }

    public void playerInteractionDropItem(Slot _slot) {
        pi.dropitem(_slot, false);  //  해당 슬롯 아이템 드롭 (슬롯, dropDisable)
    }

    public void equipmentChk(int code, Slot _slot) {
        em.chkEquipment(code, _slot);   //  장비 활성화 (무기 코드, 확인 슬롯)
    }

    public void dropEquipmentChk(int code, Slot _slot) {
        em.dropEquipmentChk(code, _slot);   //  장비 드롭 확인 (무기 코드, 확인 슬롯)
    }
    
    public void ItemImageChange(Slot _slot, bool dummyAct) {
        //  슬롯이 변경된 다음 실행될 아이템 이미지(모델) 변경 함수
        if (_slot.item.itemValue == 0) {    //  변경할 슬롯이 비어있다면
            _slot.transform.GetChild(0).gameObject.SetActive(false);    //  슬롯의 아이템이 들어갈 자식 비활성화
            if (_slot.inventoryType == ItemType.InventorySlot || _slot.inventoryType == ItemType.RestrictSlot) {    //  슬롯이 인벤토리 슬롯 또는 제한 슬롯이라면
                _slot.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);    //  아이템 개수 표시 비활성화
            }
            Destroy(_slot.transform.GetChild(0).GetChild(0).gameObject);    //  슬롯의 아이템 모델 제거
            DummyImageChange(_slot, dummyAct);  //  더미 이미지(슬롯의 배경 이미지) 변경
        }
        else {  //  변경할 슬롯에 아이템이 있다면
            for (int i = 0; i < _slot.transform.GetChild(0).childCount; i++) {  //  슬롯의 아이템이 들어갈 자식의 자식들을 모두 제거
                Destroy(_slot.transform.GetChild(0).GetChild(0).gameObject);
            }
            _slot.transform.GetChild(0).gameObject.SetActive(true); //  슬롯의 아이템이 들어갈 자식 활성화
            GameObject tempUI = _slot.item.itemUIPrefab;    //  슬롯 아이템의 UIPrefab
            GameObject tempUIItem = Instantiate(tempUI);
            tempUIItem.transform.SetParent(_slot.transform.GetChild(0));    //  아이템 모델의 부모를 슬롯으로 함
            tempUIItem.transform.localPosition = tempUI.transform.localPosition;
            tempUIItem.transform.localRotation = tempUI.transform.localRotation;
            tempUIItem.transform.localScale = tempUI.transform.localScale;
            tempUIItem.GetComponent<setUIObjectScale>().setScaleScreen();   //  스크린에 맞춰 UI 크기 조정
            tempUIItem.GetComponent<setUIObjectScale>().setScaleUi();   //  슬롯 타입에 맞춰 UI 크기 조정
            if (_slot.inventoryType == ItemType.InventorySlot) {//  슬롯이 인벤토리 슬롯이면 아이템 개수 표시
                _slot.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                _slot.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = "" + _slot.item.itemCount;
            }
            else {
                DummyImageChange(_slot, dummyAct);  //  더미 이미지(슬롯의 배경 이미지) 변경
            }
        }
    }

    private void DummyImageChange(Slot _slot, bool dummyAct) {
        //  더미 이미지(슬롯의 배경 이미지) 변경
        ItemType It = _slot.inventoryType;
        weaponSlotType wst = _slot.weaponSlotType;

        for (int i = 0; i < dummyframeslots.Count; i++) {
            if (It == dummyframeslots[i].FrameType || It == ItemType.RestrictSlot) {    //  슬롯의 타입이 현재 더미 타입과 같거나 제한슬롯이라면
                if (It == ItemType.Weapon) {    //  슬롯의 타입이 무기라면
                    if (wst == dummyframeslots[i].FrameSlotType) {  //  슬롯의 무기 슬롯 타입이 더미 무기 슬롯과 같다면
                        dummyframeslots[i].setActive(dummyAct);
                        break;
                    } else {
                        continue;
                    }
                }
                if (It != ItemType.Attachment && It != ItemType.RestrictSlot) { //  슬롯의 타입이 부착물이 아니고 제한슬롯도 아니라면
                    dummyframeslots[i].setActive(dummyAct);
                    return;
                }
                if (dummyframeslots[i].FrameType == ItemType.Attachment) {  //  현재 더미 타입이 부착물이라면
                    if (_slot.attachmentWeaponType == dummyframeslots[i].AttachmentSlot) {  //  슬롯의 부착물 위치가 더미 슬롯의 부착물 위치와 같다면
                        if (_slot.attachmentSlot == dummyframeslots[i].FrameAttachmentSlot) {   //  슬롯의 부착물 타입이 더미 슬롯의 부착물 타입과 같다면
                            if (_slot.weaponSlotType == dummyframeslots[i].FrameSlotType) { //  // (주, 보조)
                                dummyframeslots[i].setActive(dummyAct);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}