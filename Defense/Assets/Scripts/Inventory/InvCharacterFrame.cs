using UnityEngine;
using UnityEngine.EventSystems;
using Defense;

public class InvCharacterFrame : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    //  인벤토리 캐릭터 모델을 드래그로 회전하기 위해 해당 오브젝트에 커서가 있는지 확인
    [HideInInspector]
    public bool InvCharacterFrameEnter;

    public void OnPointerEnter(PointerEventData data) {
        InvCharacterFrameEnter = true;
    }

    public void OnPointerExit(PointerEventData data) {
        InvCharacterFrameEnter = false;
    }
}
