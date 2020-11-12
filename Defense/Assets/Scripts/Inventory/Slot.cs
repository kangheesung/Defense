using UnityEngine;
using UnityEngine.EventSystems;
using Defense;

public class Slot : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IEndDragHandler {
    public int number;
    [HideInInspector]
    public ItemType inventoryType;  //  무기 타입
    [HideInInspector]
    public weaponSlotType weaponSlotType;   //  무기 슬롯
    [HideInInspector]
    public ItemType attachmentWeaponType;   //  부착물 무기 타입
    [HideInInspector]
    public Attachments attachmentSlot;  //  부착물 타입

    public Item item;
    [HideInInspector]
    public Slot attachmentParentSlot;   //  부착물 부모 무기슬롯

    private bool endDragSkip;   //  드래그 스킵

    private PlayerWeaponManager pwm;

    private void Start() {
        pwm = FindObjectOfType<PlayerWeaponManager>();
        if (inventoryType == ItemType.Attachment) { //  부착물 이라면 부착물 변수의 부모를 해당 무기 슬롯으로 설정
            attachmentParentSlot = transform.parent.parent.GetChild(0).GetComponent<Slot>();
        }
    }

    public void OnDrag(PointerEventData data) {
        endDragSkip = false;
        //  마우스 휠, 우클릭 또는 제한 슬롯이라면 드래그 스킵
        if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse2) || inventoryType == ItemType.RestrictSlot) {
            endDragSkip = true;
            return;
        }
        if (this.inventoryType == ItemType.InventorySlot) {
            if (transform.childCount > 2) { //  인벤토리 슬롯에 경우 item, count, restrict 중 item의 부모를 draggingItem로 옴김 (총 자식 3개 임으로 > 2)
                transform.GetChild(0).SetParent(Inventory.instance.draggingItem);
            }
        } else if (this.inventoryType == ItemType.Attachment) {
            if (transform.childCount > 0) { //  부착물 슬롯에 경우 item의 부모를 draggingItem로 옴긴다. (총 자식 1개 임으로 > 0)
                transform.GetChild(0).SetParent(Inventory.instance.draggingItem);
            }
        } else {
            if (transform.childCount > 1) { //  그 외의 경우 두 가지의 자식 중 item의 부모를 draggingItem로 옴긴다. (총 자식 2개 임으로 > 1)
                transform.GetChild(0).SetParent(Inventory.instance.draggingItem);
            }
        }
        //  draggingItem의 자식의 포지션을 마우스 위치로 함
        Inventory.instance.draggingItem.GetChild(0).position = Inventory.instance.invCam.ScreenToWorldPoint(data.position);
        Inventory.instance.draggingItem.GetChild(0).localPosition = new Vector3(Inventory.instance.draggingItem.GetChild(0).localPosition.x, Inventory.instance.draggingItem.GetChild(0).localPosition.y, 0);
    }

    public void OnPointerEnter(PointerEventData data) {
        Inventory.instance.enteredSlot = this;
    }

    public void OnPointerExit(PointerEventData data) {
        Inventory.instance.enteredSlot = null;
    }

    public void OnEndDrag(PointerEventData data) {
        //  this == 드래그를 했을 때 선택한 슬롯
        //  Inventory.instance.draggingItem의 자식 == 드래그 중인 아이템
        //  Inventory.instance.enteredSlot == 드래그가 끝났을 때 슬롯
        if (endDragSkip) return;

        Inventory.instance.draggingItem.GetChild(0).SetParent(transform);   //  드래그가 끝났을 때 draggingItem의 자식(드래그 중인 아이템)을 this 슬롯에 자식으로 만듬
        
        for (int i = 0; i < transform.childCount; i++) {    //  slot의 자식들 순서를 유지하기 위해 슬롯에 배속된 자식 중 아이템을 찿아 첫 번째로 만듬 
            if (transform.GetChild(i).name == "item") {
                transform.GetChild(i).SetAsFirstSibling();  
            }
        }

        transform.GetChild(0).localPosition = Vector3.zero; //  슬롯에 자식인 아이템의 로컬 포지션 초기화

        //  end drag 위치가 GroundItemSlot이라면 item drop
        if (Inventory.instance.GroundItemSlot &&
            Inventory.instance.enteredSlot == null
        ) {
            Inventory.instance.playerInteractionDropItem(this);
        }

        if (Inventory.instance.enteredSlot != null) {
            if (Inventory.instance.enteredSlot == this) {   //  enter슬롯이 드래그 중인 슬롯과 같다면 리턴
                return;
            }
            if (inventoryType != Inventory.instance.enteredSlot.inventoryType) {    //  드래그중인 아이템 슬롯과 enter슬롯에 인벤토리 타입이 같지 않으면 리턴
                return;
            }
            if (Inventory.instance.enteredSlot.inventoryType == ItemType.Weapon) {  //  드래그가 끝났을 때 슬롯의 타입이 Weapon이라면
                if (pwm.Changing) { //  무기를 바꾸는 중이라면 리턴
                    return;
                }
                bool chkReturn = true;  //  chkReturn이 조건들을 지나 true면 리턴
                for (int i = 0; i < pwm.HandgunL.Count; i++) {  //  현재 무기가 Handgun이라면 false
                    if (pwm.curWeaponTransform == pwm.HandgunL[i]) {
                        chkReturn = false;
                        break;
                    }
                }
                if (pwm.curWeaponTransform == pwm.Knife) {  //  현재 무기가 Knife라면 false
                    chkReturn = false;
                }
                if (chkReturn) {
                    return;
                }
            }

            //  부착물 슬롯에서 부착물 슬롯으로 옴길 때 사용
            if (this.inventoryType == ItemType.Attachment) {    //  드래그를 했을 때 선택한 슬롯의 아이템 타입이 부착물이라면
                Inventory.instance.dropEquipmentChk(this.item.itemCode, this);  //  장비 드롭 확인
            }

            //  현재 드래그된 슬롯의 아이템과 enter슬롯의 아이템을 스왑
            Item tempItem = item;
            item = Inventory.instance.enteredSlot.item;
            Inventory.instance.enteredSlot.item = tempItem;

            if (this.inventoryType == ItemType.InventorySlot) { //  현재 드래그된 슬롯의 인벤토리 타입이 인벤토리라면
                if (Inventory.instance.enteredSlot.item.itemCode == item.itemCode) {    //  enter슬롯의 아이템 코드가 현재 드래그된 슬롯의 코드와 같다면
                    if (Inventory.instance.enteredSlot.item.itemCount + item.itemCount <= item.itemMaxCount) {  //  enter슬롯의 아이템 수 + 드래그된 슬롯의 아이템 수가 드래그된 슬롯의 아이템 최대 수보다 작거나 같다면
                        Inventory.instance.enteredSlot.item.itemCount += item.itemCount;    //  enter 슬롯의 아이템 수 증가
                        item.resetItem();   //  드래그된 슬롯의 아이템 리셋
                    }
                }
            }
            //  무기 슬롯에서 무기 슬롯으로 옴길 때 사용
            //  Inventory.instance.enteredSlot.item.itemCode + ItemDatabase.instance.itemlength : 무기 실제 코드 값
            if (this.inventoryType == ItemType.Weapon) {    //  현재 드래그된 슬롯의 인벤토리 타입이 무기라면
                Inventory.instance.dropEquipmentChk(Inventory.instance.enteredSlot.item.itemCode, this); //  장비 드롭 확인 (무기 코드, 확인 슬롯)
                if (this.item.itemValue == 1 || this.inventoryType == ItemType.Attachment) {    //  현재 드래그된 슬롯의 아이템이 존재하거나 타입이 부착물이라면
                    Inventory.instance.dropEquipmentChk(this.item.itemCode, Inventory.instance.enteredSlot); //  장비 드롭 확인 (무기 코드, 확인 슬롯)
                    Inventory.instance.equipmentChk(this.item.itemCode, this);   //  장비 활성화 (무기 코드, 확인 슬롯)
                }
                Inventory.instance.equipmentChk(Inventory.instance.enteredSlot.item.itemCode, Inventory.instance.enteredSlot);   //  장비 활성화 (무기 코드, 확인 슬롯)
            }

            if (Inventory.instance.enteredSlot.inventoryType == ItemType.Weapon) {  //  드래그가 끝났을 때 슬롯의 인벤토리 타입이 무기라면
                pwm.slingWeaponDisable();   //  sling무기 초기화
                pwm.setSling(-1, 0);    //  primary 무기 슬롯의 모습,위치 설정
                pwm.setSling(-1, 1);    //  secondary 무기 슬롯의 모습,위치 설정
            }

            //  같은 종류의 슬롯 이동 시 슬롯의 dummy 아이콘을 설정 ex) primary, secondary 슬롯
            if (this.item.itemValue == 1) {
                Inventory.instance.ItemImageChange(this, false);    //  아이템 이미지 변경, 더미 아이콘 비활성화
            } else {
                Inventory.instance.ItemImageChange(this, true); //  아이템 이미지 변경, 더미 아이콘 활성화
            }

            Inventory.instance.ItemImageChange(Inventory.instance.enteredSlot, false);  //  아이템 이미지 변경

            Inventory.instance.equipmentChk(Inventory.instance.enteredSlot.item.itemCode, Inventory.instance.enteredSlot);  //  장비 활성화 (무기 코드, 확인 슬롯)
        }
    }
}