using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponController : MonoBehaviour {

    Player player;

    public GameObject decal;
    List<GameObject> decalList = new List<GameObject>();
    public int maxDecals = 10;

    List<GameObject> bulletList = new List<GameObject>();
    public int maxBulletsOnGround = 10;

    List<GameObject> magazineList = new List<GameObject>();
    public int maxMagazinesOnGround = 10;

    public GameObject[] muzzleFlashes;

    public Transform hand;

    public enum WeaponSlot { primary, secondary }

    public Weapon currentWeapon;
    public WeaponSlot currentSlot = WeaponSlot.primary;

    public Weapon primaryWeapon;
    public Weapon secondaryWeapon;

    float rateOfFire = 0;

    float reloadTimer = 0;
    bool isReloading = false;

    float recoilFactor = 0;
    float recoilCooldownMultiplier = 1;
    public RectTransform recoilCrosshair;

    int fireMode;

    public Text ammoText;
    public Text pickUpText;

    int muzzleFlashShownFrames;

    bool pressFire = false;

	void Start () {
        player = GetComponent<Player>();
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SwapWeapon(WeaponSlot.primary);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SwapWeapon(WeaponSlot.secondary);
        }

        if (Input.GetButton(player.controlType.ToString() + " Reload") && currentWeapon != null) {
            isReloading = true;
        }

        if (isReloading) {
            reloadTimer += Time.deltaTime;
            hand.localEulerAngles = Vector3.right * 30;
            if (reloadTimer >= currentWeapon.reloadTime) {
                reloadTimer = 0;
                isReloading = false;
                hand.localEulerAngles = Vector3.zero;
                Reload();
            }
        }

        Debug.DrawRay(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward, Color.red);
        Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3, LayerMask.GetMask("Interactable"))) {
            Item item = hit.collider.GetComponent<Item>();

            if (item.interactable) {
                pickUpText.gameObject.SetActive(true);
                pickUpText.text = "[E] " + item.Message();
                //pickUpText.rectTransform.anchoredPosition = (Vector2)player.GetMovementController.playerCamera.WorldToScreenPoint(hit.collider.transform.position) - new Vector2(Screen.currentResolution.width, Screen.currentResolution.height) / 2 + new Vector2(0, 100);

                if (Input.GetButtonDown(player.controlType.ToString() + " Pickup")) {
                    item.Interact(GetComponent<Player>());
                }
            }
        } else {
            if (pickUpText.gameObject.activeSelf) {
                pickUpText.gameObject.SetActive(false);
            }
        }

        rateOfFire = Mathf.Clamp(rateOfFire - Time.deltaTime, 0, float.MaxValue);
        recoilFactor = Mathf.Clamp(recoilFactor - Time.deltaTime * recoilCooldownMultiplier, 0, float.MaxValue);

        recoilCrosshair.sizeDelta = new Vector2(50 + 100f * recoilFactor, 50 + 100f * recoilFactor);

        bool hitfire = false;

        if(player.controlType == Player.Controltype.Mouse) {
            hitfire = Input.GetButton(player.controlType.ToString() + " Fire");
        } else {
            hitfire = Input.GetAxis(player.controlType.ToString() + " Fire") > 0;
        }

        if (hitfire && currentWeapon != null && !isReloading) {
            pressFire = hitfire;
            recoilCooldownMultiplier = 1;

            if (currentWeapon.fireMode == Weapon.FireMode.Semi || currentWeapon.fireMode == Weapon.FireMode.ShotGun) {
                if (fireMode < 1) {
                    Shoot();
                }
            }
            else if (currentWeapon.fireMode == Weapon.FireMode.Burst) {
                if (fireMode < 5) {
                    Shoot();
                }
            }
            else if (currentWeapon.fireMode == Weapon.FireMode.Auto) {
                Shoot();
            }
            else if (currentWeapon.fireMode == Weapon.FireMode.Flamethrower) {
                Shoot();
            }

            if(currentWeapon.ammo <= 0) {
                if (currentWeapon != null && currentWeapon.muzzle != null && currentWeapon.muzzle.childCount > 0) {
                    if (currentWeapon.muzzle.GetChild(0).GetComponent<ParticleSystem>()) {
                        currentWeapon.muzzle.GetChild(0).GetComponent<ParticleSystem>().Stop();
                    }
                }
                fireMode = 0;
                recoilCooldownMultiplier = 4;
            }
        }

        if (pressFire && hitfire != pressFire) {
            if(currentWeapon != null && currentWeapon.muzzle != null && currentWeapon.muzzle.childCount > 0) {
                if (currentWeapon.muzzle.GetChild(0).GetComponent<ParticleSystem>()) {
                    currentWeapon.muzzle.GetChild(0).GetComponent<ParticleSystem>().Stop();
                }
            }

            fireMode = 0;
            recoilCooldownMultiplier = 4;

            pressFire = false;
        }

        if (currentWeapon != null && currentWeapon.muzzleFlash != null && currentWeapon.muzzleFlash.activeSelf) {
            muzzleFlashShownFrames++;
            if (muzzleFlashShownFrames > 3) {
                currentWeapon.muzzle.GetComponent<Light>().enabled = false;
                currentWeapon.muzzleFlash.SetActive(false);
                muzzleFlashShownFrames = 0;
            }
        }
    }

    public void PickUpGun(Weapon weapon) {
        if (weapon.preveredSlot == WeaponSlot.primary) {
            if (primaryWeapon == null) {
                primaryWeapon = weapon;
                if (currentWeapon != null)
                    currentWeapon.gameObject.SetActive(false);

                currentWeapon = primaryWeapon;
                currentWeapon.interactable = false;
                currentSlot = WeaponSlot.primary;
                UpdateAmmoCounter();

                WeaponFromFloorToHand(weapon);

                ResetReload();
            } else {
                if (currentWeapon != null)
                    currentWeapon.gameObject.SetActive(false);

                primaryWeapon.gameObject.SetActive(true);
                primaryWeapon.GetComponent<Rigidbody>().isKinematic = false;
                primaryWeapon.interactable = true;
                primaryWeapon.GetComponent<Rigidbody>().AddForce(player.GetMovementController.playerCamera.transform.forward);
                primaryWeapon.transform.parent = null;

                primaryWeapon = weapon;
                currentWeapon = primaryWeapon;
                currentWeapon.interactable = false;
                currentSlot = WeaponSlot.primary;
                UpdateAmmoCounter();

                WeaponFromFloorToHand(weapon);

                ResetReload();
            }
        } else if (weapon.preveredSlot == WeaponSlot.secondary) {
            if (secondaryWeapon == null) {
                secondaryWeapon = weapon;
                if (currentWeapon != null)
                    currentWeapon.gameObject.SetActive(false);

                currentWeapon = secondaryWeapon;
                currentSlot = WeaponSlot.secondary;
                currentWeapon.interactable = false;
                UpdateAmmoCounter();

                WeaponFromFloorToHand(weapon);

                ResetReload();
            } else {
                if (currentWeapon != null)
                    currentWeapon.gameObject.SetActive(false);

                secondaryWeapon.gameObject.SetActive(true);
                secondaryWeapon.GetComponent<Rigidbody>().isKinematic = false;
                secondaryWeapon.interactable = true;
                secondaryWeapon.GetComponent<Rigidbody>().AddForce(player.GetMovementController.playerCamera.transform.forward);
                secondaryWeapon.transform.parent = null;

                secondaryWeapon = weapon;
                currentWeapon = secondaryWeapon;
                currentWeapon.interactable = false;
                currentSlot = WeaponSlot.secondary;
                UpdateAmmoCounter();

                WeaponFromFloorToHand(weapon);

                ResetReload();
            }
        }
    }

    void WeaponFromFloorToHand(Weapon weapon) {
        weapon.GetComponent<Rigidbody>().isKinematic = true;
        weapon.transform.parent = hand;
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }

    void Reload() {
        if (currentWeapon.ammo == currentWeapon.maxAmmoMagazine)
            return;

        int needed = currentWeapon.maxAmmoMagazine - currentWeapon.ammo;

        if(currentWeapon.holdingmaxAmmo >= needed) {
            currentWeapon.ammo = currentWeapon.maxAmmoMagazine;
            currentWeapon.holdingmaxAmmo -= needed;
        }
        else {
            int remainder = currentWeapon.holdingmaxAmmo;
            currentWeapon.ammo += remainder;
            currentWeapon.holdingmaxAmmo = 0;
        }

        UpdateAmmoCounter();
    }

    void ResetReload() {
        reloadTimer = 0;
        isReloading = false;
        hand.localEulerAngles = Vector3.zero;
    }

    void Recoil() {
        FirstPersonPlayerController movement = player.GetMovementController.playerCamera.transform.parent.GetComponent<FirstPersonPlayerController>();

        recoilFactor += 0.2f * currentWeapon.affectedByRecoilFactor;

        movement.pitch -= Random.value * 1f * recoilFactor * currentWeapon.affectedByRecoilFactor;
        movement.yaw += Random.value * 1f * recoilFactor * currentWeapon.affectedByRecoilFactor;
    }

    void Shoot() {
        if (rateOfFire > 0)
            return;

        if (currentWeapon.ammo <= 0)
            return;

        fireMode++;

        Vector3 recoilOffset = new Vector3((Random.value - 0.5f) * recoilFactor * 0.15f, (Random.value - 0.5f) * recoilFactor * 0.15f, (Random.value - 0.5f) * recoilFactor * 0.15f);  

        if (currentWeapon.fireMode == Weapon.FireMode.ShotGun) {
            for (int i = 0; i < 5; i++) {
                Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward + new Vector3((Random.value - 0.5f) * 0.25f, (Random.value - 0.5f) * 0.25f, (Random.value - 0.5f) * 0.25f) + recoilOffset);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    Decal(hit);
                    Hurt(hit);
                }
            }
            MuzzleFlash();
            SpawnShell();
        }
        else if (currentWeapon.fireMode == Weapon.FireMode.Flamethrower) {
            //Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward + recoilOffset);
            //RaycastHit hit;
            //if (Physics.Raycast(ray, out hit)) {
                //Hurt(hit);
                currentWeapon.muzzle.GetChild(0).GetComponent<ParticleSystem>().Play();
            //}
        }
        else {
            Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward + recoilOffset);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                Decal(hit);
                Hurt(hit);
            }
            MuzzleFlash();
            SpawnShell();
        }

        if (currentWeapon.rateOfFire > 0) {
            rateOfFire += 60.0f / currentWeapon.rateOfFire;
        }

        currentWeapon.ammo--;

        UpdateAmmoCounter();

        Recoil();
    }

    void MuzzleFlash() {
        if (currentWeapon.muzzleFlash == null) {
            currentWeapon.muzzleFlash = Instantiate(muzzleFlashes[Random.Range(0, muzzleFlashes.Length)]);
            currentWeapon.muzzleFlash.transform.position = currentWeapon.muzzle.position;
            currentWeapon.muzzleFlash.transform.rotation = currentWeapon.muzzle.rotation;
            currentWeapon.muzzleFlash.transform.SetParent(currentWeapon.muzzle);
        } else {
            currentWeapon.muzzleFlash.SetActive(true);
            currentWeapon.muzzleFlash.transform.localEulerAngles = new Vector3(0, 0, Random.value * 360);
        }
        
        currentWeapon.muzzle.GetComponent<Light>().enabled = true;
    }

    void Hurt(RaycastHit hit) {
        if (!hit.collider.GetComponent<AgentGoTo>())
            return;

        AgentGoTo enemy = hit.collider.GetComponent<AgentGoTo>();

        enemy.health -= 20;

        if (enemy.health <= 0) {
            Destroy(enemy.gameObject);
        }
    }

    void Decal(RaycastHit hit) {
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
            if (hit.collider.GetComponent<Rigidbody>()) {
                hit.collider.GetComponent<Rigidbody>().AddForce(-hit.normal * 2000);
                return;
            }

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
            return;

        if (decalList.Count < maxDecals) {
            GameObject go = Instantiate(decal);
            //go.transform.eulerAngles = go.transform.eulerAngles - new Vector3(0, 180, 0);
            go.transform.position = hit.point + hit.normal * 0.01f;
            print(hit.point);
            go.transform.rotation = Quaternion.LookRotation(hit.normal);
            go.transform.SetParent(null);
            go.transform.localScale = Vector3.one;
            go.transform.SetParent(hit.collider.transform);
            //go.transform.localScale = new Vector3(0.2f / go.transform.lossyScale.x, 0.2f / go.transform.lossyScale.y, 0.2f / go.transform.lossyScale.z);
            decalList.Add(go);
        } else {
            GameObject go;
            if (decalList[0] != null) {
                go = decalList[0];
            } else {
                go = Instantiate(decal);
            }
            decalList.RemoveAt(0);
            go.transform.position = hit.point + hit.normal * 0.01f;
            go.transform.rotation = Quaternion.LookRotation(hit.normal);
            go.transform.SetParent(null);
            go.transform.localScale = Vector3.one;
            go.transform.SetParent(hit.collider.transform);
            //go.transform.localScale = new Vector3(0.2f / go.transform.lossyScale.x, 0.2f / go.transform.lossyScale.y, 0.2f / go.transform.lossyScale.z);
            decalList.Add(go);
        }
    }

    public void SpawnShell() {
        GameObject go = Instantiate(currentWeapon.bulletShell);
        go.transform.position = currentWeapon.transform.Find("Escape").position;
        go.transform.eulerAngles = currentWeapon.transform.eulerAngles + new Vector3(90, 0, 0);
        go.GetComponent<Rigidbody>().AddForce(currentWeapon.transform.Find("Escape").forward * 250);

        bulletList.Add(go);

        if(bulletList.Count > maxBulletsOnGround) {
            Destroy(bulletList[0]);
            bulletList.RemoveAt(0);
        }
    }

    public void UpdateAmmoCounter() {
        ammoText.text = "" + currentWeapon.ammo + "/" + currentWeapon.holdingmaxAmmo + "";
    }

    void SwapWeapon(WeaponSlot slot) {
        if (currentSlot == slot)
            return;

        ResetReload();

        if (slot == WeaponSlot.primary && primaryWeapon != null) {
            currentSlot = slot;
            DrawWeapon(primaryWeapon);
        }

        if (slot == WeaponSlot.secondary && secondaryWeapon != null) {
            currentSlot = slot;
            DrawWeapon(secondaryWeapon);
        }

        UpdateAmmoCounter();
    }

    void DrawWeapon(Weapon weapon) {
        if(currentWeapon != null)
            currentWeapon.gameObject.SetActive(false);

        currentWeapon = weapon;
        currentWeapon.gameObject.SetActive(true);

        //control.MaxAmmo = weapon.maxAmmo;
        //control.Ammo = weapon.maxAmmo;
    }
}
