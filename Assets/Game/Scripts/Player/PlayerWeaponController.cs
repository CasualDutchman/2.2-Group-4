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

    public GameObject soundObj;
    public UnityEngine.Audio.AudioMixerGroup effectsMixerGroup;

    public Transform leftIK, rightIK, combinedIK;
    Vector3 leftIKorigin, rightIKoriginPos, rightIKoriginRot, combinedIKoriginPos, combineIKoriginRot;

    public Transform hand;
    Vector3 originHandPos, originHandRot;
    bool aimDownSights = false;

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

    bool beginsound = false;
    bool endsound = false;

	void Start () {
        player = GetComponent<Player>();
        originHandPos = hand.localPosition;
        originHandRot = hand.localEulerAngles;

        rightIKoriginPos = rightIK.localPosition;
        rightIKoriginRot = rightIK.localEulerAngles;
    }
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SwapWeapon(WeaponSlot.primary);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SwapWeapon(WeaponSlot.secondary);
        }

        if (Input.GetKeyDown(KeyCode.R) && currentWeapon != null && CanReload()) {
            isReloading = true;
        }

        if (isReloading) {
            reloadTimer += Time.deltaTime;

            if(reloadTimer < 0.5f) {
                //combinedIK.localEulerAngles += Vector3.right * 40 * Time.deltaTime;
                leftIK.localPosition -= Vector3.up * 0.5f * Time.deltaTime;

                rightIK.localPosition += Vector3.up * 0.1f * Time.deltaTime;
                rightIK.localEulerAngles -= Vector3.right * 20 * Time.deltaTime;
            } else if (reloadTimer > currentWeapon.reloadTime - 0.5f) {
                //combinedIK.localEulerAngles -= Vector3.right * 40 * Time.deltaTime;
                leftIK.localPosition += Vector3.up * 0.5f * Time.deltaTime;

                rightIK.localPosition -= Vector3.up * 0.1f * Time.deltaTime;
                rightIK.localEulerAngles += Vector3.right * 20 * Time.deltaTime;
            }
            
            if (reloadTimer >= currentWeapon.reloadTime) {
                reloadTimer = 0;
                isReloading = false;
                //combinedIK.localEulerAngles = Vector3.zero;
                rightIK.localEulerAngles = rightIKoriginRot;
                rightIK.localPosition = rightIKoriginPos;
                leftIK.position = currentWeapon.secondHand.position;
                leftIK.rotation = currentWeapon.secondHand.rotation;
                Reload();
            }
        }

        Debug.DrawRay(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward, Color.red);
        Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3)) {
            Item item = hit.collider.GetComponent<Item>();

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable") || hit.collider.gameObject.layer == LayerMask.NameToLayer("InteractableCol")) {
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
        } else {
            if (pickUpText.gameObject.activeSelf) {
                pickUpText.gameObject.SetActive(false);
            }
        }

        rateOfFire = Mathf.Clamp(rateOfFire - Time.deltaTime, 0, float.MaxValue);
        recoilFactor = Mathf.Clamp(recoilFactor - Time.deltaTime * recoilCooldownMultiplier, 0, float.MaxValue);

        recoilCrosshair.sizeDelta = new Vector2(50 + 100f * recoilFactor, 50 + 100f * recoilFactor);

        if (Input.GetMouseButtonDown(1) && !isReloading) {
            //hand.localPosition -= new Vector3(originHandPos.x, 0, 0);
            combinedIK.localPosition = new Vector3(0.05f, 0, 0);
            aimDownSights = true;
            player.GetMovementController.playerCamera.fieldOfView = 50;
            player.GetMovementController.aimMultiplier = 0.5f;
        }
        if (Input.GetMouseButtonUp(1)) {
            //hand.localPosition = originHandPos;
            combinedIK.localPosition = new Vector3(0.3f, 0, 0);
            aimDownSights = false;
            player.GetMovementController.playerCamera.fieldOfView = 60;
            player.GetMovementController.aimMultiplier = 1f;
        }

        bool hitfire = false;

        if (Input.GetButtonDown(player.controlType.ToString() + " Fire")) {
            combinedIKoriginPos = combinedIK.localPosition;
        }

        if(player.controlType == Player.Controltype.Mouse) {
            hitfire = Input.GetButton(player.controlType.ToString() + " Fire");
        } else {
            hitfire = Input.GetAxis(player.controlType.ToString() + " Fire") > 0;
        }

        if (hitfire && currentWeapon != null && !isReloading && CanShoot()) {
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

                    if (currentWeapon.end != null) {
                        currentWeapon.audioSource.Stop();
                        currentWeapon.audioSource.clip = currentWeapon.end;
                        currentWeapon.audioSource.Play();
                        beginsound = false;
                        endsound = true;
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
                if (currentWeapon.end != null && !endsound) {
                    currentWeapon.audioSource.Stop();
                    currentWeapon.audioSource.clip = currentWeapon.end;
                    currentWeapon.audioSource.Play();
                    
                }
            }

            combinedIK.localPosition = combinedIKoriginPos;

            beginsound = false;
            endsound = false;

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

    bool CanShoot() {
        return currentWeapon.ammo > 0;
    }

    bool CanReload() {
        return currentWeapon.holdingmaxAmmo > 0 && currentWeapon.ammo < currentWeapon.maxAmmoMagazine;
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
                primaryWeapon.gameObject.layer = LayerMask.NameToLayer("Interactable");

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
                secondaryWeapon.gameObject.layer = LayerMask.NameToLayer("Interactable");

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
        weapon.transform.localEulerAngles = Vector3.zero;

        if (currentWeapon.secondHand != null) {
            leftIK.position = currentWeapon.secondHand.position;
            leftIK.rotation = currentWeapon.secondHand.rotation;
        }

        weapon.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
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
        hand.localEulerAngles = new Vector3(0, 0, 90);
        rightIK.localEulerAngles = rightIKoriginRot;
        rightIK.localPosition = rightIKoriginPos;
        leftIK.position = currentWeapon.secondHand.position;
        leftIK.rotation = currentWeapon.secondHand.rotation;
    }

    void Recoil() {
        FirstPersonPlayerController movement = player.GetMovementController.playerCamera.transform.parent.GetComponent<FirstPersonPlayerController>();

        float f = aimDownSights ? currentWeapon.affectedByRecoilFactor * 0.5f : currentWeapon.affectedByRecoilFactor;
        float f2 = aimDownSights ? currentWeapon.affectedByRecoilFactor * 0.8f : currentWeapon.affectedByRecoilFactor;

        f *= movement.crouched ? 0.6f : 1;
        f2 *= movement.crouched ? 0.7f : 1;

        recoilFactor += 0.2f * f;

        float shotback = Mathf.Clamp(combinedIK.localPosition.y - 1 * 0.02f * currentWeapon.affectedByRecoilFactor, -7, 1);

        combinedIK.localPosition = new Vector3(aimDownSights ? 0.05f : 0.3f, 0, shotback);

        movement.pitch -= Random.value * 1f * recoilFactor * f2;
        movement.yaw += Random.value * 1f * recoilFactor * f2;
    }

    void Shoot() {
        if (rateOfFire > 0)
            return;

        if (!CanShoot())
            return;

        fireMode++;

        Vector3 recoilOffset = new Vector3((Random.value - 0.5f) * recoilFactor * 0.15f, (Random.value - 0.5f) * recoilFactor * 0.15f, (Random.value - 0.5f) * recoilFactor * 0.15f);  

        if (currentWeapon.fireMode == Weapon.FireMode.ShotGun) {
            for (int i = 0; i < 5; i++) {
                Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward + new Vector3((Random.value - 0.5f) * 0.25f, (Random.value - 0.5f) * 0.25f, (Random.value - 0.5f) * 0.25f) + recoilOffset);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    Hit(hit, true, true);
                }
            }
            MuzzleFlash();
            SpawnShell();
            Sound();
        }
        else if (currentWeapon.fireMode == Weapon.FireMode.Flamethrower) {
            Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward + recoilOffset);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 5)) {
                Hit(hit, false, false);
            }
            currentWeapon.muzzle.GetChild(0).GetComponent<ParticleSystem>().Play();
            Sound();
        }
        else {
            Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward + recoilOffset);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                Hit(hit, true, true);
            }
            MuzzleFlash();
            SpawnShell();
            Sound();
        }

        if (currentWeapon.rateOfFire > 0) {
            rateOfFire += 60.0f / currentWeapon.rateOfFire;
        }

        currentWeapon.ammo--;

        UpdateAmmoCounter();

        Recoil();
    }

    void Sound() {
        if (currentWeapon.begin == null && currentWeapon.end == null) {
            GameObject shotSoundobj = Instantiate(soundObj, currentWeapon.muzzle.position, Quaternion.identity);
            shotSoundobj.transform.SetParent(currentWeapon.muzzle);
            shotSoundobj.transform.localPosition = Vector3.zero;

            AudioSource source = shotSoundobj.GetComponent<AudioSource>();

            source.outputAudioMixerGroup = effectsMixerGroup;
            source.clip = currentWeapon.shoot;
            source.Play();
            //currentWeapon.audioSource.Play();
        }

        if ((currentWeapon.audioSource != null && currentWeapon.audioSource.isPlaying) || !CanShoot())
            return;

        if (currentWeapon.end != null && beginsound) {
            currentWeapon.audioSource.clip = currentWeapon.shoot;
            currentWeapon.audioSource.Play();
        }

        if (currentWeapon.begin != null && !beginsound) {
            currentWeapon.audioSource.Stop();
            currentWeapon.audioSource.clip = currentWeapon.begin;
            currentWeapon.audioSource.Play();
            beginsound = true;
        }
    }

    void Hit(RaycastHit hit, bool decal, bool hurt) {
        if (hit.collider.GetComponent<Barrel>()) {
            hit.collider.GetComponent<Barrel>().health--;
            if (hit.collider.GetComponent<Barrel>().health <= 0) {
                hit.collider.GetComponent<Barrel>().Explode();
                return;
            }
        }

        if(hurt)
            Hurt(hit, decal);
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

    void Hurt(RaycastHit hit, bool decal) {
        if (!hit.collider.GetComponent<EnemyPart>()) {
            if (decal)
                Decal(hit);

            return;
        }

        //AgentGoTo enemy = hit.collider.GetComponent<AgentGoTo>();

        //enemy.health -= 20;

        EnemyPart part = hit.collider.GetComponent<EnemyPart>();
        part.Damage(10);

        Enemy enemy = part.connected;

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

        //go.layer = LayerMask.NameToLayer("Ignore Raycast");

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
