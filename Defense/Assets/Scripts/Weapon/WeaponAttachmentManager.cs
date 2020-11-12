using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttachmentManager : MonoBehaviour {
    //  UI attachment manager

    //  무기 UI attachment에 사용
    public List<Transform> attachmentTransformList; //  부착물 리스트

    private void Start() {
        //  이 오브젝트의 자식들을 리스트에 추가
        for (int i = 0; i < this.transform.childCount; i++) {
            attachmentTransformList.Add(this.transform.GetChild(i));
        }
    }
}
