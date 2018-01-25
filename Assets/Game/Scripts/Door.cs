using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class Door : Item {

    //What to open
    public string itemName;

    //current state of the door
    public bool open = false;

    //future state of the door
    bool moveToState = false;

    //Time to close or open the object
    public float openCloseTime = 1;

    //different states of the objct. Can also be a drawer moving in and out, instead of just rotating
    public Transform closedStateTransform, openStateTransform;

    //When the object rotated differently, because the rotation is negative and the door might swing in the other direction, going trough walls.
    //This can be checked to change the direction of the swing.
    public bool negativeOpenState = false;

    //Change the curve of opening, this makes the movement less linear(if the curve is not linear)
    public AnimationCurve curve;

    //When another door is part of the opening
    public Door connectedDoor;

    float timer;

    void Start() {
        if (open) {//Change state when the begin is open
            transform.localPosition = openStateTransform.localPosition;
            transform.localEulerAngles = openStateTransform.localEulerAngles;
            if (connectedDoor) {
                connectedDoor.transform.localPosition = openStateTransform.localPosition;
                connectedDoor.transform.localEulerAngles = openStateTransform.localEulerAngles;
            }
        }
    }

    //message
    public override string Message() {
        return (open ? "Close" : "Open") + " " + itemName;
    }

    //interaction callback
    public override void Interact(Player player) {
        ChangeState();

        if (connectedDoor) {
            connectedDoor.ChangeState();
        }
    }

    public void ChangeState() {
        if (open == moveToState)
            moveToState = !moveToState;
    }

    void Update () {
		if(moveToState != open) {

            if (moveToState) {//open object
                transform.localPosition = Vector3.Lerp(closedStateTransform.localPosition, openStateTransform.localPosition, curve.Evaluate(timer));

                transform.localEulerAngles = Vector3.Lerp(closedStateTransform.localEulerAngles, openStateTransform.localEulerAngles * (negativeOpenState ? -1 : 1), curve.Evaluate(timer));
            } else {//close object
                transform.localPosition = Vector3.Lerp(openStateTransform.localPosition, closedStateTransform.localPosition, curve.Evaluate(timer));

                transform.localEulerAngles = Vector3.Lerp(openStateTransform.localEulerAngles * (negativeOpenState ? -1 : 1), closedStateTransform.localEulerAngles, curve.Evaluate(timer));
            }
            //lerp is used, to move in the animationCurve. But the curve can be nonlinear

            if (timer >= 1) {
                timer = 0;
                open = moveToState;
                return;
            }
            timer += Time.deltaTime / openCloseTime;
        }
	}
}
