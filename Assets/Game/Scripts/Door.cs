using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public bool open = false;

    bool moveToState = false;

    public Transform left, right;

    public AnimationCurve curve;

    Vector3 rightclosed = new Vector3(0.75f, 2f, 0f);
    Vector3 rightopen = new Vector3(1.5f, 2f, 0f);
    Vector3 leftclosed = new Vector3(-0.75f, 2f, 0f);
    Vector3 leftopen = new Vector3(-1.5f, 2f, 0f);

    float timer;

    [ContextMenu("Change state")]
    public void ChangeState() {
        if(open == moveToState)
            moveToState = !moveToState;
    }

	void Start () {
		
	}
	
	void Update () {
		if(moveToState != open) {
            
            if (moveToState) {
                left.localPosition = Vector3.Lerp(leftclosed, leftopen, curve.Evaluate(timer));
                right.localPosition = Vector3.Lerp(rightclosed, rightopen, curve.Evaluate(timer));

                left.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, 90, 0), curve.Evaluate(timer));
                right.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, -90, 0), curve.Evaluate(timer));

                if (timer >= 1) {
                    timer = 0;
                    open = moveToState;
                    return;
                }
            } else {
                left.localPosition = Vector3.Lerp(leftopen, leftclosed, curve.Evaluate(timer));
                right.localPosition = Vector3.Lerp(rightopen, rightclosed, curve.Evaluate(timer));

                left.localEulerAngles = Vector3.Lerp(new Vector3(0, 90, 0), Vector3.zero, curve.Evaluate(timer));
                right.localEulerAngles = Vector3.Lerp(new Vector3(0, -90, 0), Vector3.zero, curve.Evaluate(timer));

                if (timer >= 1) {
                    timer = 0;
                    open = moveToState;
                    return;
                }
            }
            timer += Time.deltaTime;
        }
	}
}
