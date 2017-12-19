using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonPlayerController : MonoBehaviour {

    CharacterController controller;
    Player player;
    public Camera playerCamera;

    public float mouseSpeed = 5;
    public float aimMultiplier = 1;
    public float yaw = 0.0f;
    public float pitch = 0.0f;

    public bool invertY = false;

    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    public bool crouched = false;
    Vector3 originCameraPos;

    void Start () {
        controller = GetComponent<CharacterController>();
        player = GetComponent<Player>();
        playerCamera = transform.GetChild(0).GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;

        originCameraPos = playerCamera.transform.localPosition;

        foreach (string s in Input.GetJoystickNames()) {
            print(s);
        }

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Raycast"), LayerMask.NameToLayer("Interactable"));
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Raycast"), LayerMask.NameToLayer("Water"));
    }

    void Update () {
        if (Time.deltaTime > 0) {
            yaw += Input.GetAxis(player.controlType.ToString() + " X") * mouseSpeed * aimMultiplier;
            pitch = Mathf.Clamp(pitch + (Input.GetAxis(player.controlType.ToString() + " Y") * mouseSpeed * aimMultiplier * (invertY ? -1 : 1)), -90, 90);
        }
        transform.GetChild(0).localEulerAngles = new Vector3(pitch, 0, 0);
        transform.localEulerAngles = new Vector3(0, yaw, 0);

        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            crouched = true;
            playerCamera.transform.localPosition -= new Vector3(0, 0.3f, 0);
        }
        if (Input.GetKeyUp(KeyCode.LeftControl)) {
            crouched = false;
            playerCamera.transform.localPosition = originCameraPos;
        }

        //walking around
        if (controller.isGrounded) {
            moveDirection = new Vector3(Input.GetAxis(player.controlType.ToString() + " Horizontal"), 0, Input.GetAxis(player.controlType.ToString() + " Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= crouched ? speed * 0.5f : speed * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1);
            if (Input.GetButton(player.controlType.ToString() + " Jump"))
                moveDirection.y = jumpSpeed;

        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}
