using System;
using UnityEngine;

public class CameraTargetMove : MonoBehaviour {
    public string HorizontalInputAxis = "Horizontal";
    public string VerticalInputAxis = "Vertical";

    public float MoveSpeed;
    public float FastMoveSpeed;
    public KeyCode FastMoveKeyCode1;
    public KeyCode FastMoveKeyCode2;

    protected void Update() {
        var speed = MoveSpeed;
        if (Input.GetKey(FastMoveKeyCode1) || Input.GetKey(FastMoveKeyCode2)) {
            speed = FastMoveSpeed;
        }

        var h = Input.GetAxisRaw(HorizontalInputAxis);
        if (Mathf.Abs(h) > 0.001f) {
            AddToPosition(h * speed * Time.deltaTime, 0, 0);
        }

        var v = Input.GetAxisRaw(VerticalInputAxis);
        if (Mathf.Abs(v) > 0.001f) {
            AddToPosition(0, 0, v * speed * Time.deltaTime);
        }
    }

    private void AddToPosition(float dx, float dy, float dz) {
        transform.position += new Vector3(dx, dy, dz);
    }
}
