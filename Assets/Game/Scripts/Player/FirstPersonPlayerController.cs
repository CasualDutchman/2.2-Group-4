using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonPlayerController : MonoBehaviour {

    CharacterController controller;
    Player player;
    public Camera playerCamera;

    public float mouseSpeed = 5;
    public float yaw = 0.0f;
    public float pitch = 0.0f;

    public bool invertY = false;

    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    void Start () {
        controller = GetComponent<CharacterController>();
        player = GetComponent<Player>();
        playerCamera = transform.GetChild(0).GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update () {
        yaw += Input.GetAxis(player.controlType.ToString() + " X") * mouseSpeed;
        pitch = Mathf.Clamp(pitch + (Input.GetAxis(player.controlType.ToString() + " Y") * mouseSpeed * (invertY ? -1 : 1)), -90, 90);

        transform.GetChild(0).localEulerAngles = new Vector3(pitch, 0, 0);
        transform.localEulerAngles = new Vector3(0, yaw, 0);

        //walking around
        if (controller.isGrounded) {
            moveDirection = new Vector3(Input.GetAxis(player.controlType.ToString() + " Horizontal"), 0, Input.GetAxis(player.controlType.ToString() + " Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton(player.controlType.ToString() + " Jump"))
                moveDirection.y = jumpSpeed;

        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}
