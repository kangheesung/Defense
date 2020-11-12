using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Defense;

public class GroundItemSlot : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerClickHandler {
    public Item item;

    public int code;

    private bool endDragSkip;   //  드래그 스킵
    
    private PlayerInteraction pi;
    private PlayerWeaponManager pwm;

    private void Start() {
        pi = FindObjectOfType<PlayerInteraction>();
        pwm = FindObjectOfType<PlayerWeaponManager>();
    }

    public void OnPointerClick(PointerEventData eventData) {
        //  해당 슬롯 우클릭시 아이템 pickup
        if (eventData.button == PointerEventData.InputButton.Right) {
            pi.itemPickUp();
        }
    }

    public void OnDrag(PointerEventData data) {
        endDragSkip = false;
        if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse2)) { //  마우스 휠, 우클릭 시 스킵
            endDragSkip = true;
            return;
        }
        if (transform.parent != Inventory.instance.draggingItem) {  //  해당 슬롯의 부모를 draggingItem으로옴김
            transform.SetParent(Inventory.instance.draggingItem);
        }
        //  draggingItem의 자식의 포지션을 마우스 위치로 함
        Inventory.instance.draggingItem.GetChild(0).position = Inventory.instance.invCam.ScreenToWorldPoint(data.position);
        Inventory.instance.draggingItem.GetChild(0).localPosition = new Vector3(Inventory.instance.draggingItem.GetChild(0).localPosition.x, Inventory.instance.draggingItem.GetChild(0).localPosition.y, 0);
    }

    public void OnEndDrag(PointerEventData data) {
        //  this == 드래그를 했을 때 선택한 슬롯
        //  Inventory.instance.draggingItem의 자식 == 드래그 중인 아이템
        //  Inventory.instance.enteredSlot == 드래그가 끝났을 때 슬롯
        if (endDragSkip) return;

        Inventory.instance.draggingItem.GetChild(0).SetParent(pi.ScrollContent);    //  드래그가 끝났을 때 draggingItem의 자식(드래그 중인 아이템)을 원래 위치 (ScrollContent)의 자식으로 옴김

        if (Inventory.instance.enteredSlot != null) {
            if (Inventory.instance.enteredSlot.inventoryType == ItemType.Attachment) {  //  enterSlot이 부착물이고
                if (item.itemAttachment != Inventory.instance.enteredSlot.attachmentSlot) { //  부착물의 종류가 다르다면 리턴
                    return;
                }
            }

            if (item.itemType != Inventory.instance.enteredSlot.inventoryType) {    //  아이템 타입이 enterSlot의 타입과 다르고
                if (item.itemType != ItemType.Consumption || Inventory.instance.enteredSlot.inventoryType != ItemType.InventorySlot) {  //  타입이 Cinsumption 이거나 인벤토리 타입이 아니라면 리턴
                    return;
                }
            }

            if (item.itemType == ItemType.InventorySlot || item.itemType == ItemType.Consumption) { //  아이템이 인벤토리 타입이거나 consumption 타입이고
                if (Inventory.instance.enteredSlot.item.itemCode == item.itemCode) {    //  현재 아이템과 enterSlot의 아이템 코드가 같고
                    if (Inventory.instance.enteredSlot.item.itemCount + item.itemCount <= item.itemMaxCount) {  //  enterSlot의 아이템 수 + 현재 아이템 수가 현재 아이템의 Max값 보다 같거나 작다면
                        item.itemCount += Inventory.instance.enteredSlot.item.itemCount;    //  현재 아이템의 수 값에 enterSlot 아이템 수를 더함
                    }
                }
            } else {    //  인벤토리 타입, consumption 타입이 아니라면
                if (Inventory.instance.enteredSlot.item.itemCode == item.itemCode) {    //  현재 아이템과 enterSlot의 아이템 코드가 같다면 리턴
                    return;
                }
            }

            Inventory.instance.enteredSlot.item = item; //  enterSlot의 아이템을 현재 아이템으로 바꿈
            Inventory.instance.ItemImageChange(Inventory.instance.enteredSlot, false);  //  아이템 이미지 변경, 더미 아이콘 비활성화

            if (Inventory.instance.enteredSlot.inventoryType == ItemType.Weapon) {  //  드래그가 끝났을 때 슬롯의 인벤토리 타입이 무기라면
                pwm.setSling(-1, 0);    //  primary 무기 슬롯의 모습,위치 설정
                pwm.setSling(-1, 1);    //  secondary 무기 슬롯의 모습,위치 설정
            }

            for (int i = 0; i < pi.tf.Count; i++) {
                pi.tf[i].GetComponent<GroundItemSlot>().code = i;   //  GroundItemSlot의 코드를 순서대로 i로 설정
            }

            pi.tf.RemoveAt(code);
            Destroy(gameObject);
            DropItem di = pi.di[code];
            pi.di.RemoveAt(code);
            di.DestroyObject();
            Inventory.instance.equipmentChk(di.code, Inventory.instance.enteredSlot);   //  장비 활성화 (무기 코드, 확인 슬롯)
        }
    }
}