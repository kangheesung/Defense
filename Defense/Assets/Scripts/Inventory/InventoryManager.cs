using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour {
    public static bool showInventory = false;   //  인벤토리 상태 (true : 열림, false : 닫힘)

    public Canvas inventoryCanvas;  
    public GameObject UICameraRig;  //  인벤토리 UI 카메라 리그

    //  이벤트
    [System.Serializable]
    public class OnInventoryOpen : UnityEvent { }
    [System.Serializable]
    public class OnInventoryClose : UnityEvent { }
    public OnInventoryOpen OnOpen;
    public OnInventoryClose OnClose;

    private InputManager im;
    private PlayerWeapon pw;
    private InvRotateCharacter irc;

    private void Start() {
        im = FindObjectOfType<InputManager>();
        irc = FindObjectOfType<InvRotateCharacter>();
        InventoryClose();
    }

    private void Update() {
        //  인벤토리 입력
        for (int i = 0; i < im.Inventory.Length; i++) {
            if (Input.GetKeyDown(im.Inventory[i])) {
                showInventory = !showInventory;
                if (showInventory) {
                    InventoryOpen();
                }
                else {
                    InventoryClose();
                }
                break;
            }
        }
    }

    private void InventoryOpen() {
        inventoryCanvas.enabled = true; //  캔버스 활성화
        UICameraRig.SetActive(true);    //  카메라 리그 활성화
        irc.resetRotation();    //  인벤토리 카메라 회전 초기화
    }

    private void InventoryClose() {
        inventoryCanvas.enabled = false;    //  캔버스 비활성화
        UICameraRig.SetActive(false);   //  카메라 리그 비활성화
        irc.resetRotation();    //  인벤토리 카메라 회전 초기화
    }
}