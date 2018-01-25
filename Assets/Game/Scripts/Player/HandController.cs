using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class HandController : MonoBehaviour {

    //The animations of the hand is done with Inverse Kinematics.
    //This way all the animations can be made in code, because only the IK transforms need to move

    Animator anim;

    public float ikWieght = 1;

    public Transform leftIKTarget;
    public Transform rightIKTarget;

    public Transform leftHint;
    public Transform rightHint;

    void Start () {
        anim = GetComponent<Animator>();
	}
	
    void OnAnimatorIK() {
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWieght);
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWieght);

        anim.SetIKPosition(AvatarIKGoal.LeftHand, leftIKTarget.position);
        anim.SetIKPosition(AvatarIKGoal.RightHand, rightIKTarget.position);

        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWieght);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWieght);

        anim.SetIKRotation(AvatarIKGoal.LeftHand, leftIKTarget.rotation);
        anim.SetIKRotation(AvatarIKGoal.RightHand, rightIKTarget.rotation);
    }
}
