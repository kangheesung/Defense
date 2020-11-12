using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    [Header("Movement keys")]
    public string HorizontalAxis = "Horizontal";
    public string VerticalAxis = "Vertical";
    public KeyCode[] Crouch;          //  C
    public KeyCode[] Sprint;          //  LeftShift
    public KeyCode[] Roll;            //  Space

    [Header("Gameplay keys")]
    public KeyCode[] Fire;            //  Mouse0
    public KeyCode[] Aim;             //  Mouse1
    public KeyCode[] Grenade;         //  G
    public KeyCode[] Use;             //  E
    public KeyCode[] Reload;          //  R
    public KeyCode[] Firemode;        //  B
    public KeyCode[] Inventory;       //  Tab, I

    private void Start() {
        Cursor.lockState = CursorLockMode.Confined;
    }
}