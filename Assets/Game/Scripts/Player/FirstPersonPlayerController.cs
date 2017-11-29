using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonPlayerController : MonoBehaviour {

    CharacterController controller;

    public float mouseSpeed = 5;
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    public bool invertY = false;

    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    void Start () {
        controller = GetComponent<CharacterController>();

        //Cursor.lockState = CursorLockMode.Locked;
    }
	
	void Update () {
        //looking around

        //print(Input.GetKeyDown("joystick button 0"));

        //yaw += Input.get * mouseSpeed;
        yaw += Input.GetAxis("Mouse X") * mouseSpeed;
        pitch = Mathf.Clamp(pitch + (Input.GetAxis("Mouse Y") * mouseSpeed * (invertY ? -1 : 1)), -90, 90);

        transform.GetChild(0).localEulerAngles = new Vector3(pitch, 0, 0);
        transform.localEulerAngles = new Vector3(0, yaw, 0);

        //walking around
        if (controller.isGrounded) {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;

        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}
