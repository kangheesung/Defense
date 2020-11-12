using UnityEngine;
using Defense;

public class DropItem : MonoBehaviour {
    public int code;    //  아이템 코드
    public int count;   //  아이템 개수

    public string itemName; //  아이템 이름

    private void Start() {
        resetCode();    //  이름을 기반으로 아이템 코드 리셋
    }

    public void resetCode() {   //  Database에서 이름이 같은 item을 찿아 그 코드로 설정
        for (int i = 0; i < ItemDatabase.instance.items.Count; i++) {
            if (itemName == ItemDatabase.instance.items[i].itemName) {
                code = i;
                break;
            }
        }
    }

    public void DestroyObject() {   //  파괴
        Destroy(gameObject.transform.parent.gameObject);
    }
}