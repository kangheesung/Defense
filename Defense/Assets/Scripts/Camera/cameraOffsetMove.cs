using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraOffsetMove : MonoBehaviour {
    private Vector3 localOffset;
    private PlayerMovement pm;
    private InputManager im;

    private void Start() {
        localOffset = transform.localPosition;

        pm = FindObjectOfType<PlayerMovement>();
        im = FindObjectOfType<InputManager>();
    }

    private void LateUpdate() {
        for (int i = 0; i < im.Crouch.Length; i++) {
            for (int j = 0; j < im.Aim.Length; j++) {
                if (Input.GetKey(im.Crouch[i]) && Input.GetKey(im.Aim[j])) {    //  앉기와 Aim을 동시에 누르면 캐릭터와 Aim 사이에 카메라를 이동
                    float targetDistance = Mathf.Clamp(Vector3.Distance(transform.parent.position, pm.AimFinalPos.position) / 2, -10, 10);  //  카메라 이동을 제한함
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, targetDistance), 10 * Time.deltaTime);
                    return;
                }
            }
        }
        transform.localPosition = localOffset;
    }
}