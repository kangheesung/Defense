using UnityEngine;
using UnityEditor;
using Defense;

[CustomEditor(typeof(Slot))]
public class InspectorEditor_Slot : Editor {
    public override void OnInspectorGUI() {
        Slot slot = (Slot)target;
        if (slot.inventoryType != ItemType.RestrictSlot) {  //  제한슬롯이 아니면
            DrawDefaultInspector(); //  기존 인스펙터 출력
        }
        slot.inventoryType = (ItemType)EditorGUILayout.EnumPopup("Inventory Type", slot.inventoryType);
        if (slot.inventoryType == ItemType.Weapon || slot.inventoryType == ItemType.Attachment) {   //  무기슬롯 또는 부착물 슬롯 이라면
            slot.weaponSlotType = (weaponSlotType)EditorGUILayout.EnumPopup("WeaponSlotType", slot.weaponSlotType); //  무기 슬롯 출력(첫 번째, 두 번째)
        }
        if (slot.inventoryType == ItemType.Attachment) {    //  부착물 슬롯 이라면
            slot.attachmentParentSlot = (Slot)EditorGUILayout.ObjectField("attachmentParentSlot", slot.attachmentParentSlot, typeof(Slot), true);
            slot.attachmentWeaponType = (ItemType)EditorGUILayout.EnumPopup("AttachmentWeaponType", slot.attachmentWeaponType);
            slot.attachmentSlot = (Attachments)EditorGUILayout.EnumPopup("AttachmentSlot", slot.attachmentSlot);
        }
    }
}