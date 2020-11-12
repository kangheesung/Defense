using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvRotateCharacter : MonoBehaviour {
    private Vector3 posLastFame;
    private InvCharacterFrame icf;
    public bool DragOn;
    public bool MouseButtonDown;

    private void Start() {
        icf = FindObjectOfType<InvCharacterFrame>();
    }

    private void Update() {
        if ((!icf.InvCharacterFrameEnter || MouseButtonDown) && !DragOn) {
            if (Input.GetMouseButton(0)) {
                MouseButtonDown = true;
            } else {
                MouseButtonDown = false;
            }
        }
        if ((icf.InvCharacterFrameEnter || DragOn) && !MouseButtonDown) {
            DragOn = false;
            if (Input.GetMouseButtonDown(0)) {
                posLastFame = Input.mousePosition;
            }
            if (Input.GetMouseButton(0)) {
                DragOn = true;

                var delta = Input.mousePosition - posLastFame;
                posLastFame = Input.mousePosition;

                var axis = Quaternion.AngleAxis(90f, Vector3.forward) * delta;
                transform.rotation = Quaternion.AngleAxis(delta.magnitude * 0.5f, axis) * transform.rotation;
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            }
        }
    }

    public void resetRotation() {
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
}
