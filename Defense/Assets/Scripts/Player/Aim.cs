using UnityEngine;

public class Aim : MonoBehaviour {
    private Animator anim;

    private void Start() {
        anim = GetComponent<Animator>();
    }

    public void aimReduce(bool act) {
        anim.SetBool("AimReduce", act);
    }

    public void aimShake() {
        anim.SetTrigger("AimShake");
    }

    public void aimHeight(float height) {
        //  Aim 높이 설정
        transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, 0, height);
    }
}
