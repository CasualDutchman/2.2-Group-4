using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonPlayerController : MonoBehaviour {

    public AudioClip[] footsteps;
    public GameObject soundObject;
    int lastStepIndex;

    CharacterController controller;
    Player player;
    public Camera playerCamera;

    public float aimMultiplier = 1;
    public float yaw = 0.0f;
    public float pitch = 0.0f;

    public bool invertY = false;

    public float speed = 6.0F;
    public float crouchSpeed = 1.0f;
    public float sprintSpeed = 10.0f;
    public float crouchSprintSpeed = 4.0f;

    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    bool jumping = false;

    public bool walking = false;
    public bool sprinting = false;
    public bool crouched = false;
    Vector3 originCameraPos;

    public float stamina = 100;
    public Image staminaImage;

    float walkTimer = 0;

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
        float mouseSpeed = OptionsMenu.instance != null ? OptionsMenu.instance.mouseSpeed : 4;

        if (Time.deltaTime > 0) {
            yaw += Input.GetAxis(player.controlType.ToString() + " X") * mouseSpeed * aimMultiplier;
            pitch = Mathf.Clamp(pitch + (Input.GetAxis(player.controlType.ToString() + " Y") * mouseSpeed * aimMultiplier * (invertY ? -1 : 1)), -90, 90);
        }
        transform.GetChild(0).localEulerAngles = new Vector3(pitch, 0, 0);
        transform.localEulerAngles = new Vector3(0, yaw, 0);

        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            crouched = true;
            playerCamera.transform.localPosition = new Vector3(0, 1, 0);
        }
        if (Input.GetKeyUp(KeyCode.LeftControl)) {
            crouched = false;
            playerCamera.transform.localPosition = originCameraPos;
        }

        sprinting = Input.GetKey(KeyCode.LeftShift);

        if (sprinting && walking) {
            stamina -= Time.deltaTime * 10;
            if(stamina <= 0) {
                sprinting = false;
            }
        }else{
            stamina = Mathf.Clamp(stamina += Time.deltaTime * 12, 0, 100f);
        }

        staminaImage.fillAmount = stamina / 100.0f;

        //walking around
        if (controller.isGrounded) {
            if (jumping) {
                StartCoroutine(JumpFallSound());
                jumping = false;
            }
            moveDirection = new Vector3(Input.GetAxis(player.controlType.ToString() + " Horizontal"), 0, Input.GetAxis(player.controlType.ToString() + " Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= crouched ? (sprinting ? crouchSprintSpeed : crouchSpeed) : (sprinting ? sprintSpeed : speed);
            if (Input.GetButton(player.controlType.ToString() + " Jump")) {
                moveDirection.y = jumpSpeed;
                jumping = true;
            }
        }

        walking = moveDirection.magnitude > 0.2f;

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);

        if(walking && controller.isGrounded)
            WalkUpdate();
    }

    void WalkUpdate() {
        walkTimer += Time.deltaTime;

        float current = sprinting ? 0.3f : (crouched ? 1f : (walking ? 0.5f : 0));

        if (walkTimer >= current) {
            walkTimer -= current;

            GameObject go = Instantiate(soundObject, transform.position, Quaternion.identity);
            AudioSource source = go.GetComponent<AudioSource>();
            int index = Random.Range(0, footsteps.Length);
            if(index == lastStepIndex) {
                index = Random.Range(0, footsteps.Length);
            }
            source.clip = footsteps[index];
            source.volume = sprinting ? 1f : (crouched ? 0.1f : 0.7f);
            source.Play();

            lastStepIndex = index;
        }
    }

    IEnumerator JumpFallSound() {
        int firstindex;
        int index;

        GameObject go = Instantiate(soundObject, transform.position, Quaternion.identity);
        AudioSource source = go.GetComponent<AudioSource>();
        index = firstindex = Random.Range(0, footsteps.Length);
        source.clip = footsteps[index];
        source.volume = 1f;
        source.Play();

        yield return new WaitForSeconds(0.02f);

        go = Instantiate(soundObject, transform.position, Quaternion.identity);
        source = go.GetComponent<AudioSource>();
        index = Random.Range(0, footsteps.Length);
        if (index == firstindex) {
            index = Random.Range(0, footsteps.Length);
        }
        source.clip = footsteps[index];
        source.volume = 1f;
        source.Play();
    }

    public bool CanSee(Collider collider) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        bool b1 = GeometryUtility.TestPlanesAABB(planes, collider.bounds);
        bool b2 = false;

        RaycastHit hit;
        if (Physics.Linecast(playerCamera.transform.position, collider.bounds.center, out hit)) {
            if (hit.collider.name == collider.name) {
                b2 = true;
            }
        }
        return b1 && b2;
    }
}
