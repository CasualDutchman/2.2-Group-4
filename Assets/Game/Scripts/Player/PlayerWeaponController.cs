using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponController : MonoBehaviour {

    public GameObject decal;
    List<GameObject> decalList = new List<GameObject>();
    public int maxDecals = 10;

    List<GameObject> bulletList = new List<GameObject>();
    public int maxBulletsOnGround = 10;

    List<GameObject> magazineList = new List<GameObject>();
    public int maxMagazinesOnGround = 10;

    public Transform hand;

    public enum WeaponSlot { primary, secondary }

    public Weapon currentWeapon;
    public WeaponSlot currentSlot = WeaponSlot.primary;

    public Weapon primaryWeapon;
    public Weapon secondaryWeapon;

    FirstPersonPlayerController control;

    float rateOfFire = 0;

    float recoilFactor = 0;
    public RectTransform recoilCrosshair;

    int fireMode;
    Transform muzzle;

    public Text ammoText;
    public Text pickUpText;

	void Start () {
        control = GetComponent<FirstPersonPlayerController>();

        if(currentWeapon != null) {
            muzzle = currentWeapon.transform.Find("Muzzle");
        }
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SwapWeapon(WeaponSlot.primary);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SwapWeapon(WeaponSlot.secondary);
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            Reload();
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3) && hit.collider.CompareTag("Gun")) {
            Weapon weapon = hit.collider.GetComponent<Weapon>();
            if (weapon != null) {
                pickUpText.gameObject.SetActive(true);
                pickUpText.text = "[E] Pick up " + weapon.weaponName;
                pickUpText.rectTransform.anchoredPosition = (Vector2)Camera.main.WorldToScreenPoint(hit.collider.transform.position) - new Vector2(Screen.currentResolution.width, Screen.currentResolution.height) / 2;

                if (Input.GetKeyDown(KeyCode.E)) {
                    if (weapon.preveredSlot == WeaponSlot.primary) {
                        if (primaryWeapon == null) {
                            primaryWeapon = weapon;
                            if (currentWeapon != null)
                                currentWeapon.gameObject.SetActive(false);

                            currentWeapon = primaryWeapon;
                            currentSlot = WeaponSlot.primary;
                            UpdateAmmoCounter();

                            weapon.GetComponent<Rigidbody>().isKinematic = true;
                            weapon.transform.parent = hand;
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = Quaternion.identity;
                        }
                        else {
                            if (currentWeapon != null)
                                currentWeapon.gameObject.SetActive(false);

                            primaryWeapon.gameObject.SetActive(true);
                            primaryWeapon.GetComponent<Rigidbody>().isKinematic = false;
                            primaryWeapon.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward);
                            primaryWeapon.transform.parent = null;

                            primaryWeapon = weapon;
                            currentWeapon = primaryWeapon;
                            currentSlot = WeaponSlot.primary;
                            UpdateAmmoCounter();

                            weapon.GetComponent<Rigidbody>().isKinematic = true;
                            weapon.transform.parent = hand;
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = Quaternion.identity;
                        }
                    }
                    else if (weapon.preveredSlot == WeaponSlot.secondary) {
                        if(secondaryWeapon == null) {
                            secondaryWeapon = weapon;
                            if (currentWeapon != null)
                                currentWeapon.gameObject.SetActive(false);

                            currentWeapon = secondaryWeapon;
                            currentSlot = WeaponSlot.secondary;
                            UpdateAmmoCounter();

                            weapon.GetComponent<Rigidbody>().isKinematic = true;
                            weapon.transform.parent = hand;
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = Quaternion.identity;
                        } else {
                            if (currentWeapon != null)
                                currentWeapon.gameObject.SetActive(false);

                            secondaryWeapon.gameObject.SetActive(true);
                            secondaryWeapon.GetComponent<Rigidbody>().isKinematic = false;
                            secondaryWeapon.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward);
                            secondaryWeapon.transform.parent = null;

                            secondaryWeapon = weapon;
                            currentWeapon = secondaryWeapon;
                            currentSlot = WeaponSlot.secondary;
                            UpdateAmmoCounter();

                            weapon.GetComponent<Rigidbody>().isKinematic = true;
                            weapon.transform.parent = hand;
                            weapon.transform.localPosition = Vector3.zero;
                            weapon.transform.localRotation = Quaternion.identity;
                        }
                    }
                }
            }
        } else {
            if (pickUpText.gameObject.activeSelf) {
                pickUpText.gameObject.SetActive(false);
            }
        }

        rateOfFire = Mathf.Clamp(rateOfFire - Time.deltaTime, 0, float.MaxValue);
        recoilFactor = Mathf.Clamp(recoilFactor - Time.deltaTime, 0, float.MaxValue);

        recoilCrosshair.sizeDelta = new Vector2(100 + 100f * recoilFactor, 100 + 100f * recoilFactor);

        if (Input.GetMouseButton(0) && currentWeapon != null) {
            if(currentWeapon.fireMode == Weapon.FireMode.Semi || currentWeapon.fireMode == Weapon.FireMode.ShotGun) {
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
        }

        if (Input.GetMouseButtonUp(0)) {
            fireMode = 0;
        }
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

    void Recoil() {
        FirstPersonPlayerController movement = Camera.main.transform.parent.GetComponent<FirstPersonPlayerController>();

        recoilFactor += 0.2f;

        movement.pitch -= Random.value * 0.7f * recoilFactor;
        movement.yaw += Random.value * 0.7f * recoilFactor;
    }

    void Shoot() {
        if (rateOfFire > 0)
            return;

        if (currentWeapon.ammo <= 0)
            return;

        fireMode++;

        if (currentWeapon.fireMode != Weapon.FireMode.ShotGun) {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                Decal(hit);
                Hurt(hit);
            }
        }else {
            for (int i = 0; i < 5; i++) {
                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward + new Vector3((Random.value - 0.5f) * 0.25f, (Random.value - 0.5f) * 0.25f, (Random.value - 0.5f) * 0.25f));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    Decal(hit);
                    Hurt(hit);
                }
            }
        }

        SpawnShell();

        if (currentWeapon.rateOfFire > 0) {
            rateOfFire += 60.0f / currentWeapon.rateOfFire;
        }

        currentWeapon.ammo--;

        UpdateAmmoCounter();

        Recoil();
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
        if (decalList.Count < maxDecals) {
            GameObject go = Instantiate(decal);
            //go.transform.eulerAngles = go.transform.eulerAngles - new Vector3(0, 180, 0);
            go.transform.position = hit.point + hit.normal * 0.01f;
            go.transform.rotation = Quaternion.LookRotation(hit.normal);
            go.transform.SetParent(hit.collider.transform);
            decalList.Add(go);
        } else {
            GameObject go = decalList[0];
            decalList.RemoveAt(0);
            go.transform.position = hit.point + hit.normal * 0.01f;
            go.transform.rotation = Quaternion.LookRotation(hit.normal);
            go.transform.SetParent(hit.collider.transform);
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

    void UpdateAmmoCounter() {
        ammoText.text = "" + currentWeapon.ammo + "/" + currentWeapon.holdingmaxAmmo + "";
    }

    void SwapWeapon(WeaponSlot slot) {
        if (currentSlot == slot)
            return;

        if(slot == WeaponSlot.primary && primaryWeapon != null) {
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
        muzzle = currentWeapon.transform.Find("Muzzle");

        //control.MaxAmmo = weapon.maxAmmo;
        //control.Ammo = weapon.maxAmmo;
    }
}
