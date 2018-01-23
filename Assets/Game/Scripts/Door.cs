using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Item {

    public string itemName;

    public bool open = false;

    public float openCloseTime = 1;

    bool moveToState = false;

    public Transform closedStateTransform, openStateTransform;

    public bool negativeOpenState = false;

    public AnimationCurve curve;

    public Door connectedDoor;

    float timer;

    void Start() {
        if (open) {
            transform.localPosition = openStateTransform.localPosition;
            transform.localEulerAngles = openStateTransform.localEulerAngles;
            if (connectedDoor) {
                connectedDoor.transform.localPosition = openStateTransform.localPosition;
                connectedDoor.transform.localEulerAngles = openStateTransform.localEulerAngles;
            }
        }
    }

    public override string Message() {
        return (open ? "Close" : "Open") + " " + itemName;
    }

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

            if (moveToState) {
                transform.localPosition = Vector3.Lerp(closedStateTransform.localPosition, openStateTransform.localPosition, curve.Evaluate(timer));

                transform.localEulerAngles = Vector3.Lerp(closedStateTransform.localEulerAngles, openStateTransform.localEulerAngles * (negativeOpenState ? -1 : 1), curve.Evaluate(timer));
            } else {
                transform.localPosition = Vector3.Lerp(openStateTransform.localPosition, closedStateTransform.localPosition, curve.Evaluate(timer));

                transform.localEulerAngles = Vector3.Lerp(openStateTransform.localEulerAngles * (negativeOpenState ? -1 : 1), closedStateTransform.localEulerAngles, curve.Evaluate(timer));
            }

            if (timer >= 1) {
                timer = 0;
                open = moveToState;
                return;
            }
            timer += Time.deltaTime / openCloseTime;
        }
	}
}
