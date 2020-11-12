using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Defense;

public class PlayerInteraction : MonoBehaviour {
    public Transform GroundSlot; 
    public Transform ScrollContent;
    public GameObject DropItem; //  드롭 아이템을 저장할 프리팹
    public List<DropItem> di;   //  dropItem리스트
    public List<Transform> tf;  //  groundSlot Transform

    public bool disableDropitem;    //  아이템 드롭 제한

    private int primaryWeaponSlot;  //  주무기 슬롯 번호
    private int secondaryWeaponSlot;    //  부무기 슬롯 번호

    private PlayerWeaponManager pwm;

    private void Awake() {
        di = new List<DropItem>();
        tf = new List<Transform>();

        pwm = FindObjectOfType<PlayerWeaponManager>();
    }

    private void Start() {
        //  주, 부 무기 슬롯 번호 설정
        for (int i = 0; i < Inventory.instance.equipmentSlots.Count; i++) {
            if (Inventory.instance.equipmentSlots[i].GetChild(0).GetComponent<Slot>().inventoryType == ItemType.Weapon) {
                if (Inventory.instance.equipmentSlots[i].GetChild(0).GetComponent<Slot>().weaponSlotType == weaponSlotType.Primary) {
                    primaryWeaponSlot = Inventory.instance.equipmentSlots[i].GetChild(0).GetComponent<Slot>().number;
                }
                else if (Inventory.instance.equipmentSlots[i].GetChild(0).GetComponent<Slot>().weaponSlotType == weaponSlotType.Secondary) {
                    secondaryWeaponSlot = Inventory.instance.equipmentSlots[i].GetChild(0).GetComponent<Slot>().number;
                }
            }
        }
    }
    
    private void OnTriggerEnter(Collider other) {
        //  GroundSlot 생성
        if (other.tag == "Item") {
            di.Add(other.GetComponent<DropItem>());
            Transform tempTransform = Instantiate(GroundSlot);
            tempTransform.SetParent(ScrollContent);
            tempTransform.localPosition = new Vector3(tempTransform.localPosition.x, tempTransform.localPosition.y, 0);
            tempTransform.localRotation = Quaternion.Euler(0, 0, 0);
            tempTransform.localScale = new Vector3(1, 1, 1);
            tempTransform.GetComponent<GroundItemSlot>().item = new Item(ItemDatabase.instance.items[other.GetComponent<DropItem>().code]); //  GroundItemSlot아이템 설정
            //  slot Ui 설정
            GameObject tempUI = tempTransform.GetComponent<GroundItemSlot>().item.itemUIPrefab;
            GameObject tempUIItem = Instantiate(tempUI);
            tempUIItem.transform.SetParent(tempTransform.transform.GetChild(0));
            tempUIItem.transform.localPosition = tempUI.transform.localPosition;
            tempUIItem.transform.localRotation = tempUI.transform.localRotation;
            tempUIItem.transform.localScale = tempUI.transform.localScale;
            tempUIItem.GetComponent<setUIObjectScale>().setScaleScreen();
            tempUIItem.GetComponent<setUIObjectScale>().setScaleUi();
            tempTransform.GetComponent<GroundItemSlot>().item.itemCount = di.Last().count;
            //  아이템 개수 설정
            if (tempTransform.GetComponent<GroundItemSlot>().item.itemType != ItemType.Consumption) {
                tempTransform.GetChild(1).GetChild(0).gameObject.SetActive(false);
            } else {
                tempTransform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                tempTransform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = "" + tempTransform.GetComponent<GroundItemSlot>().item.itemCount;
            }
            tf.Add(tempTransform);
        }
    }

    private void OnTriggerExit(Collider other) {
        //  GroundSlot 제거
        if (other.tag == "Item" && tf.Count != 0 && di.Count != 0) {
            for (int i = 0; i < di.Count; i++) {
                if (di[i] == other.GetComponent<DropItem>()) {
                    di.RemoveAt(i);
                    Destroy(tf[i].gameObject);
                    tf.RemoveAt(i);
                    return;
                }
            }
            di.RemoveAt(0);
            Destroy(tf.First().gameObject);
            tf.RemoveAt(0);
        }
    }

    private void Update() {
        //  PickUp
        if (Input.GetKeyDown(KeyCode.E) && !disableDropitem) {
            if (!pwm.Changing) {
                itemPickUp();
            }
        }
    }

    public void itemPickUp() {
        if (di.Count != 0) {
            DropItem tempDi = di.First();
            Inventory.instance.AddItem(tempDi.code, tempDi.count);  //  아이템 추가
            if (Inventory.instance.EquipmentSlotFull) { //  인벤토리가 꽉찼으면 리턴
                return;
            }
            if (ItemDatabase.instance.items[tempDi.code].itemType == ItemType.Weapon) { //  캐릭터 앞 뒤 무기 모델 설정
                pwm.setSling(-1, 0);
                pwm.setSling(-1, 1);
            }
            Inventory.instance.equipmentChk(tempDi.code, Inventory.instance.itemPickUpTempSlot);    //  장비 활성화 (무기 코드, 확인 슬롯)
            di.RemoveAt(0);
            tempDi.DestroyObject();
            Destroy(tf.First().gameObject);
            tf.RemoveAt(0);
        }
    }

    public void dropitem (Slot _slot, bool disableIgnore) {
        if ((disableDropitem || pwm.Changing) && !disableIgnore) {  //  disableDropitem 또는 Changing 이 참이고 disableIgnore 참이라면 리턴
            return;
        }

        //  아이템이 드랍으로 리셋되기전에 코드 번호를 임시로 담음
        int code = -1;
        if (_slot.inventoryType == ItemType.Weapon) {
            code = _slot.item.itemCode;
        }

        //  아이템 드랍
        Vector3 tempVec = new Vector3(transform.position.x, 0, transform.position.z);
        Quaternion tempQuaternion = transform.rotation;
        tempQuaternion.x = 0;
        tempQuaternion.z = 0;
        GameObject tempitem = Instantiate(_slot.item.itemPrefab, tempVec, tempQuaternion);
        tempitem.transform.GetChild(0).GetComponent<DropItem>().resetCode();
        tempitem.transform.GetChild(0).GetComponent<DropItem>().count = _slot.item.itemCount;
        tempitem.transform.parent = DropItem.transform;
        Inventory.instance.dropEquipmentChk(tempitem.transform.GetChild(0).GetComponent<DropItem>().code, _slot);
        Inventory.instance.removeItem(_slot);

        //  캐릭터 장비 모델 설정
        if (_slot.inventoryType == ItemType.Weapon) {
            if (_slot.number == primaryWeaponSlot) {
                pwm.setSling(code, 0);
            } else if (_slot.number == secondaryWeaponSlot) {
                pwm.setSling(code, 1);
            }
            pwm.curWeaponDropChk();
        }
        if (_slot.inventoryType == ItemType.Handgun) {
            pwm.curWeaponDropChk();
        }
    }
}