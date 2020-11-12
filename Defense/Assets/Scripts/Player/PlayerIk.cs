using UnityEngine;
using System;
using System.Collections;
using Defense;

[RequireComponent(typeof(Animator))]

public class PlayerIk : MonoBehaviour {
    protected Animator anim;

    public bool ikActive = true;
    public Transform[] leftHandTargets;
    public Transform leftHandTarget = null;

    void Start() {
        anim = GetComponent<Animator>();
    }

    //a callback for calculating IK
    void OnAnimatorIK() {
        if (anim) {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive) {
                // Set the right hand target position and rotation, if one has been assigned
                if (leftHandTarget != null) {
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
                }
            }
            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                anim.SetLookAtWeight(0);
            }
        }
    }

    public void ikChange(Transform tf) {
        leftHandTarget = tf;
        ikActive = true;
    }

    public void ikChange(Weapon cw) {
        switch(cw) {
            case Weapon.Knife:
                ikActive = false;
                break;
            case Weapon.Pistol:
                ikActive = true;
                leftHandTarget = leftHandTargets[0];
                break;
            case Weapon.Rifle:
                ikActive = true;
                leftHandTarget = leftHandTargets[1];
                break;
            case Weapon.Machinegun:
                ikActive = true;
                leftHandTarget = leftHandTargets[2];
                break;
            case Weapon.Rocketlauncher:
                ikActive = true;
                leftHandTarget = leftHandTargets[3];
                break;
        }
    }
}
