using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defense;

public class EquipmentManager : MonoBehaviour {
    public Transform[] Heads;
    public Transform[] Bodys;
    public Transform[] Equipments;
    public Transform[] Backpacks;
    public Transform[] Legs;

    public Transform DefaultHead;
    public Transform DefaultBody;
    public Transform DefaultLeg;

    private Transform curHead;
    private Transform curBody;
    public Transform curEquipment;
    public Transform curBackpack;
    private Transform curLeg;

    private int num;

    private PlayerEquipment pe;
    private PlayerWeaponManager pwm;

    private void Awake() {
        pe = FindObjectOfType<PlayerEquipment>();
        pwm = FindObjectOfType<PlayerWeaponManager>();

        InitArmor(Heads, ItemType.Helmet);
        InitArmor(Bodys, ItemType.Chest);
        InitArmor(Equipments, ItemType.Equipment);
        InitArmor(Backpacks, ItemType.Backpack);
        InitArmor(Legs, ItemType.Pant);
    }

    private void Start() {
        resetEquipment();   //  장비를 모두 비활성화

        curHead = DefaultHead;
        curBody = DefaultBody;
        curEquipment = null;
        curBackpack = null;
        curLeg = DefaultLeg;

        curHead.gameObject.SetActive(true);
        curBody.gameObject.SetActive(true);
        curLeg.gameObject.SetActive(true);
    }

    private void InitArmor(Transform[] armor, ItemType itemtype) {  //  장비의 정보를 데이터 베이스에 추가
        string name;
        string desc;
        GameObject prefab;
        GameObject uiPrefab;
        for (int i = 0; i < armor.Length; i++) {
            armor[i].GetComponent<PlayerEquipment>().equipmentCode = num;
            name = armor[i].GetComponent<PlayerEquipment>().equipmentName;
            desc = armor[i].GetComponent<PlayerEquipment>().equipmentDesc;
            prefab = armor[i].GetComponent<PlayerEquipment>().equipmentPrefab;
            uiPrefab = armor[i].GetComponent<PlayerEquipment>().equipmentUIPrefab;
            if (prefab != null && uiPrefab != null) {
                ItemDatabase.instance.Add(name, 1, num, desc, itemtype, Attachments.LowerMount, 1, prefab, uiPrefab);
            } else {
                ItemDatabase.instance.Add(name, 1, num, desc, itemtype, Attachments.LowerMount, 1, null, Inventory.instance.dummyPrefab);
            }
            num++;
        }
    }

    public void chkEquipment(int _code, Slot _slot) {
        Item item = ItemDatabase.instance.items[_code]; //  아이템 데이터 베이스를 이용해 코드를 Item으로 변환
        switch (item.itemType) {
            case ItemType.Weapon:
            case ItemType.Handgun:
                //  아이템 타입이 무기라면
                PlayerWeapon tempWeapon;
                if (_slot.weaponSlotType == weaponSlotType.Primary) {   // 주무기 슬롯이라면 클론이 비활성화된 Transform 반환
                    tempWeapon = pwm.returnWeaponTransform(_code, false).GetComponent<PlayerWeapon>();
                } else {    //  부무기 슬롯이라면 클론이 활성화된 Transform 반환
                    tempWeapon = pwm.returnWeaponTransform(_code, true).GetComponent<PlayerWeapon>();
                }
                if (tempWeapon.weaponStats.weaponMuzzle) {  //  tempWeapon에 Muzzle이 활성화 되있다면
                    setAttachmentSlot(_slot, Attachments.Muzzle);
                }
                if (tempWeapon.weaponStats.weaponLowerMount) {
                    setAttachmentSlot(_slot, Attachments.LowerMount);
                }
                if (tempWeapon.weaponStats.weaponSight) {
                    setAttachmentSlot(_slot, Attachments.Sight);
                }
                if (tempWeapon.weaponStats.weaponMagazine) {
                    setAttachmentSlot(_slot, Attachments.Magazine);
                }
                break;
            case ItemType.Attachment:
                PlayerWeapon tempAttachmentWeapon;
                if (_slot.weaponSlotType == weaponSlotType.Primary) {   // 주무기 슬롯이라면 클론이 비활성화된 Transform 반환
                    tempAttachmentWeapon = pwm.returnWeaponTransform(_slot.transform.parent.parent.GetChild(0).GetComponent<Slot>().item.itemCode, false).GetComponent<PlayerWeapon>();
                } else {    //  부무기 슬롯이라면 클론이 활성화된 Transform 반환
                    tempAttachmentWeapon = pwm.returnWeaponTransform(_slot.transform.parent.parent.GetChild(0).GetComponent<Slot>().item.itemCode, true).GetComponent<PlayerWeapon>();
                }
                for (int i = 0; i < tempAttachmentWeapon.weaponAttachmentData.Count; i++) {
                    if (tempAttachmentWeapon.weaponAttachmentData[i].GetComponent<PlayerItem>().code == _code) {
                        tempAttachmentWeapon.weaponAttachmentTransform[i].gameObject.SetActive(true);   //  무기의 해당 부착물 활성화
                        if (_slot.transform.parent.parent.GetChild(0).GetChild(0).GetChild(0) != null) {
                            WeaponAttachmentManager tempWAM = _slot.transform.parent.parent.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<WeaponAttachmentManager>();    //  WeaponAttachmentManager 참조 설정
                            for (int j = 0; j < tempWAM.attachmentTransformList.Count; j++) {
                                if (tempWAM.attachmentTransformList[j].name == _slot.item.itemName) {   //  WAM에서 추가하려는 아이템과 같은 이름을 가진 오브젝트를 활성화(UI)
                                    tempWAM.attachmentTransformList[j].gameObject.SetActive(true);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
                break;
            case ItemType.Helmet:
                curEquipmentChange(Heads, ref curHead, item.itemCode);
                break;
            case ItemType.Chest:
                curEquipmentChange(Bodys, ref curBody, item.itemCode);
                break;
            case ItemType.Equipment:
                curEquipmentChange(Equipments, ref curEquipment, item.itemCode);
                break;
            case ItemType.Backpack:
                curEquipmentChange(Backpacks, ref curBackpack, item.itemCode);
                break;
            case ItemType.Pant:
                curEquipmentChange(Legs, ref curLeg, item.itemCode);
                break;
            default:
                break;
        }
        pwm.slingWeaponDisable();   //  sling 초기화
        pwm.setSling(-1, 0);    //  sling 설정
        pwm.setSling(-1, 1);
    }

    private void setAttachmentSlot(Slot _slot, Attachments _attachmentType) {
        for (int i = 1; i < _slot.transform.parent.childCount - 1; i++) {
            if (_slot.transform.parent.GetChild(i).GetChild(0).GetComponent<Slot>().attachmentSlot == _attachmentType) {
                _slot.transform.parent.GetChild(i).GetChild(0).GetComponent<Slot>().inventoryType = ItemType.Attachment;    //  해당 슬롯의 인벤토리 타입을 Attachment로 변경
                _slot.transform.parent.GetChild(i).GetChild(1).gameObject.SetActive(false); //  제한 이미지를 비활성화
                break;
            }
        }
    }

    public void dropEquipmentChk(int _code, Slot _slot) {
        switch (_slot.inventoryType) {
            case ItemType.Weapon:
            case ItemType.Handgun:
                //  아이템 슬롯 타입이 무기라면
                PlayerWeapon tempWeapon;
                if (_slot.weaponSlotType == weaponSlotType.Primary) {
                    tempWeapon = pwm.returnWeaponTransform(_code, false).GetComponent<PlayerWeapon>();  // 주무기 슬롯이라면 클론이 비활성화된 Transform 반환
                } else {
                    tempWeapon = pwm.returnWeaponTransform(_code, true).GetComponent<PlayerWeapon>();   //  부무기 슬롯이라면 클론이 활성화된 Transform 반환
                }
                if (tempWeapon.weaponStats.weaponMuzzle) {  //  tempWeapon에 Muzzle이 활성화 되있다면
                    setDropAttachmentSlot(_slot, Attachments.Muzzle, tempWeapon);
                }
                if (tempWeapon.weaponStats.weaponLowerMount) {
                    setDropAttachmentSlot(_slot, Attachments.LowerMount, tempWeapon);
                }
                if (tempWeapon.weaponStats.weaponSight) {
                    setDropAttachmentSlot(_slot, Attachments.Sight, tempWeapon);
                }
                if (tempWeapon.weaponStats.weaponMagazine) {
                    setDropAttachmentSlot(_slot, Attachments.Magazine, tempWeapon);
                }
                break;
            case ItemType.Attachment:
                PlayerWeapon tempAttachmentWeapon;
                if (_slot.weaponSlotType == weaponSlotType.Primary) {
                    tempAttachmentWeapon = pwm.returnWeaponTransform(_slot.transform.parent.parent.GetChild(0).GetComponent<Slot>().item.itemCode, false).GetComponent<PlayerWeapon>(); // 주무기 슬롯이라면 클론이 비활성화된 Transform 반환
                } else {
                    tempAttachmentWeapon = pwm.returnWeaponTransform(_slot.transform.parent.parent.GetChild(0).GetComponent<Slot>().item.itemCode, true).GetComponent<PlayerWeapon>();  //  부무기 슬롯이라면 클론이 활성화된 Transform 반환
                }
                for (int i = 0; i < tempAttachmentWeapon.weaponAttachmentData.Count; i++) {
                    if (tempAttachmentWeapon.weaponAttachmentData[i].GetComponent<PlayerItem>().code == _code) {
                        tempAttachmentWeapon.weaponAttachmentTransform[i].gameObject.SetActive(false);  //  부착물 비활성화(캐릭터 모델)
                        if (_slot.transform.parent.parent.GetChild(0).GetChild(0).GetChild(0) != null) {
                            WeaponAttachmentManager tempWAM = _slot.transform.parent.parent.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<WeaponAttachmentManager>();
                            for (int j = 0; j < tempWAM.attachmentTransformList.Count; j++) {
                                if (tempWAM.attachmentTransformList[j].name == ItemDatabase.instance.items[_code].itemName) {
                                    tempWAM.attachmentTransformList[j].gameObject.SetActive(false); //  부착물 비활성화(UI)
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
                break;
            case ItemType.Helmet:
                curHead.gameObject.SetActive(false);
                curHead = DefaultHead;
                curHead.gameObject.SetActive(true);
                break;
            case ItemType.Chest:
                curBody.gameObject.SetActive(false);
                curBody = DefaultBody;
                curBody.gameObject.SetActive(true);
                break;
            case ItemType.Equipment:
                curEquipment.gameObject.SetActive(false);
                Inventory.instance.slotRestrict(Inventory.instance.inventoryX, Inventory.instance.inventoryY, Inventory.instance.inventoryX - 2, true);
                curEquipment = null;
                break;
            case ItemType.Backpack:
                curBackpack.gameObject.SetActive(false);
                Inventory.instance.slotRestrict(Inventory.instance.inventoryX, Inventory.instance.inventoryY, Inventory.instance.inventoryX - 1, true);
                Inventory.instance.slotRestrict(Inventory.instance.inventoryX, Inventory.instance.inventoryY, Inventory.instance.inventoryX - 0, true);
                curBackpack = null;
                break;
            case ItemType.Pant:
                curLeg.gameObject.SetActive(false);
                curLeg = DefaultLeg;
                curLeg.gameObject.SetActive(true);
                break;
            default:
                break;
        }
        pwm.slingWeaponDisable();   //  sling 초기화
        pwm.setSling(-1, 0);    //  sling 설정
        pwm.setSling(-1, 1);
    }

    private void setDropAttachmentSlot(Slot _slot, Attachments _attachmentType, PlayerWeapon tempWeapon) {
        for (int i = 1; i < _slot.transform.parent.childCount - 1; i++) {
            if (_slot.transform.parent.GetChild(i).GetChild(0).GetComponent<Slot>().attachmentSlot == _attachmentType) {
                _slot.transform.parent.GetChild(i).GetChild(0).GetComponent<Slot>().inventoryType = ItemType.RestrictSlot;
                _slot.transform.parent.GetChild(i).GetChild(1).gameObject.SetActive(true);
                if (_slot.transform.parent.GetChild(i).GetChild(0).GetComponent<Slot>().item.itemValue != 0) {
                    ///*    weaponMagazine - X ?
                    int tempCode = _slot.transform.parent.GetChild(i).GetChild(0).GetComponent<Slot>().item.itemCode;   //  부착물 슬롯의 아이템 코드 저장
                    for (int j = 0; j < tempWeapon.weaponAttachmentData.Count; j++) {
                        if (tempWeapon.weaponAttachmentData[j].GetComponent<PlayerItem>().code == tempCode) {
                            tempWeapon.weaponAttachmentTransform[j].gameObject.SetActive(false);    //  부착물 비활성화(캐릭터 모델)
                            if (_slot.transform.GetChild(0).GetChild(0).GetChild(0) != null) {
                                WeaponAttachmentManager tempWAM = _slot.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<WeaponAttachmentManager>();
                                for (int k = 0; k < tempWAM.attachmentTransformList.Count; k++) {
                                    if (tempWAM.attachmentTransformList[k].name == ItemDatabase.instance.items[tempCode].itemName) {
                                        tempWAM.attachmentTransformList[k].gameObject.SetActive(false); //  부착물 비활성화(UI)
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                    //*/
                    Inventory.instance.playerInteractionDropItem(_slot.transform.parent.GetChild(i).GetChild(0).GetComponent<Slot>());  //  해당 부착물 드랍
                }
            }
        }
    }

    private void curEquipmentChange(Transform[] _equipment, ref Transform _curEquipment, int tempCode) {
        for (int i = 0; i < _equipment.Length; i++) {
            if (_equipment[i].GetComponent<PlayerEquipment>().equipmentCode == tempCode) {
                if (_curEquipment != null) {
                    _curEquipment.gameObject.SetActive(false);  //  현재 장비 비활성화
                }
                _equipment[i].gameObject.SetActive(true);   //  바꿀 장비 활성화
                _curEquipment = _equipment[i];  //  현재 장비를 바꿀 장비로 설정
                if (_curEquipment != null) {
                    switch (_curEquipment.GetComponent<PlayerEquipment>().equipmentType) {  //  장비에 따라 인벤토리 크기 변경
                        case ItemType.Equipment:
                            Inventory.instance.slotRestrict(Inventory.instance.inventoryX, Inventory.instance.inventoryY, Inventory.instance.inventoryX - 2, false);
                            break;
                        case ItemType.Backpack:
                            Inventory.instance.slotRestrict(Inventory.instance.inventoryX, Inventory.instance.inventoryY, Inventory.instance.inventoryX - 0, false);
                            Inventory.instance.slotRestrict(Inventory.instance.inventoryX, Inventory.instance.inventoryY, Inventory.instance.inventoryX - 1, false);
                            break;
                    }
                }
            }
        }
    }

    private void resetEquipment() { //  장비를 모두 비활성화
        for (int i = 0; i < Heads.Length; i++) {
            Heads[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < Bodys.Length; i++) {
            Bodys[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < Equipments.Length; i++) {
            Bodys[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < Backpacks.Length; i++) {
            Bodys[i].gameObject.SetActive(false);
        }
    }
}
