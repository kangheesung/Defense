using UnityEngine;
using UnityEngine.EventSystems;

public class DropGroundItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    //  GroundSlot에 아이템을 드래그하여 drop하기 위해 해당 오브젝트에 커서가 있는지 확인
    public void OnPointerEnter(PointerEventData data) {
        Inventory.instance.GroundItemSlot = true;
    }

    public void OnPointerExit(PointerEventData data) {
        Inventory.instance.GroundItemSlot = false;
    }
}