using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defense;

public class AnimationEvent : StateMachineBehaviour {
    public bool Ik; //  ik 재설정
    public bool rollEnd;    //  구르기 끝났을 때
    public bool reloadORGrenadeEnd; //  장전, 던지기 끝났을 때
    public bool MShootEnd;  //  근접 공격 끝났을 때
    public bool DrawStart;  //  draw 시작
    public bool DrawEnd;    //  draw 끝

    private PlayerIk pik;
    private PlayerMovement pm;
    private PlayerWeapon pw;
    private PlayerWeaponManager pwm;

    private void Awake() {
        pik = FindObjectOfType<PlayerIk>();
        pm = FindObjectOfType<PlayerMovement>();
        pw = FindObjectOfType<PlayerWeapon>();
        pwm = FindObjectOfType<PlayerWeaponManager>();
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (DrawStart) {
            pwm.animationDrawStart();
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (rollEnd) {
            pm.rollActive(false);
        }
        if (reloadORGrenadeEnd) {
            pwm.curWeaponTransform.GetComponent<PlayerWeapon>().shootingActive(true);
        }
        if (Ik) {
            if (pwm.curWeaponTransform == pwm.Knife) {
                pik.ikActive = false;
            } else {
                pik.ikActive = true;
            }
        }
        if (MShootEnd) {
            pm.disableMovement = false;
            pw.meleeAttack = false;
        }
        if (DrawEnd) {
            pwm.animationDrawEnd();
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
