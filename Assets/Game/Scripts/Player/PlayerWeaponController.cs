using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponController : MonoBehaviour {

    Player player;

    //decal details
    public GameObject decal;
    List<GameObject> decalList = new List<GameObject>();
    public int maxDecals = 10;

    //bullet details
    List<GameObject> bulletList = new List<GameObject>();
    public int maxBulletsOnGround = 10;

    //random muzzleflashes
    public GameObject[] muzzleFlashes;

    //sound info
    public GameObject soundObj;
    public UnityEngine.Audio.AudioMixerGroup effectsMixerGroup;

    //IK transforms. This is used for simple code-based animations
    public Transform leftIK, rightIK, combinedIK;
    Vector3 leftIKorigin, rightIKoriginPos, rightIKoriginRot, combinedIKoriginPos, combineIKoriginRot;

    public Transform hand;
    Vector3 originHandPos, originHandRot;
    bool aimDownSights = false;

    public enum WeaponSlot { primary, secondary }

    //weapon info
    public Weapon currentWeapon;
    public WeaponSlot currentSlot = WeaponSlot.primary;

    public Weapon primaryWeapon;
    public Weapon secondaryWeapon;

    //timers and stuff
    float rateOfFire = 0;

    float reloadTimer = 0;
    bool isReloading = false;

    //recoild info
    float recoilFactor = 0;
    float recoilCooldownMultiplier = 1;
    public RectTransform recoilCrosshair;

    public AnimationCurve meleeAnimationCurve;

    int fireMode;

    public Text ammoText;
    public Text pickUpText;

    int muzzleFlashShownFrames;

    bool pressFire = false; // true if mouse if held
    public bool shooting = false;// true, only when shooting (false if no ammo but still holding mouse)

    //sound info if weapon has multiple sound stages
    bool beginsound = false;
    bool endsound = false;

    //set origins
	void Start () {
        player = GetComponent<Player>();
        originHandPos = hand.localPosition;
        originHandRot = hand.localEulerAngles;

        leftIKorigin = leftIK.localPosition;

        rightIKoriginPos = rightIK.localPosition;
        rightIKoriginRot = rightIK.localEulerAngles;
    }
	
    //this is where all the magic happens
	void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {//when '1' is pressed, swap to primary weapon
            SwapWeapon(WeaponSlot.primary);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {//when '2' is pressed, swap to secondary weapon
            SwapWeapon(WeaponSlot.secondary);
        }

        if (Input.GetKeyDown(KeyCode.R) && currentWeapon != null && CanReload()) { // when 'R' is pressed, start reloading
            isReloading = true;

            combinedIK.localPosition = new Vector3(0.3f, 0, 0);
            aimDownSights = false;
            player.GetMovementController.playerCamera.fieldOfView = 60;
            player.GetMovementController.aimMultiplier = 1f;
        }

        if (isReloading) {  //reloading animations for the IK
            reloadTimer += Time.deltaTime;

            if(reloadTimer < 0.5f) {
                leftIK.localPosition -= Vector3.up * 0.5f * Time.deltaTime;

                rightIK.localPosition += Vector3.up * 0.1f * Time.deltaTime;
                rightIK.localEulerAngles -= Vector3.right * 20 * Time.deltaTime;
            } else if (reloadTimer > currentWeapon.reloadTime - 0.5f) {
                leftIK.localPosition += Vector3.up * 0.5f * Time.deltaTime;

                rightIK.localPosition -= Vector3.up * 0.1f * Time.deltaTime;
                rightIK.localEulerAngles += Vector3.right * 20 * Time.deltaTime;
            }
            
            if (reloadTimer >= currentWeapon.reloadTime) {
                reloadTimer = 0;
                isReloading = false;
                rightIK.localEulerAngles = rightIKoriginRot;
                rightIK.localPosition = rightIKoriginPos;
                leftIK.position = currentWeapon.secondHand.position;
                leftIK.rotation = currentWeapon.secondHand.rotation;
                Reload();
            }
        }

        //Always check in front of the player for interactable objects
        Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3)) {
            Item item = hit.collider.GetComponent<Item>();

            //Check if on right layer
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable") || hit.collider.gameObject.layer == LayerMask.NameToLayer("InteractableCol")) {
                if (item.interactable) {
                    pickUpText.gameObject.SetActive(true);
                    pickUpText.text = "[E] " + item.Message();

                    //When the interact button is pressed
                    if (Input.GetButtonDown(player.controlType.ToString() + " Pickup")) {
                        item.Interact(GetComponent<Player>());
                    }
                }
            } else {//disable the pickup text
                if (pickUpText.gameObject.activeSelf) {
                    pickUpText.gameObject.SetActive(false);
                }
            }
        } else {//disable the pickup text
            if (pickUpText.gameObject.activeSelf) {
                pickUpText.gameObject.SetActive(false);
            }
        }

        //Make sure values are always above 0
        rateOfFire = Mathf.Clamp(rateOfFire - Time.deltaTime, 0, float.MaxValue);
        recoilFactor = Mathf.Clamp(recoilFactor - Time.deltaTime * recoilCooldownMultiplier, 0, float.MaxValue);

        //Change the crosshair based on recoil
        recoilCrosshair.sizeDelta = new Vector2(50 + 100f * recoilFactor, 50 + 100f * recoilFactor);

        if (Input.GetMouseButtonDown(1) && !isReloading && CanAimDownSights()) { //When starting to aim down sights
            combinedIK.localPosition = new Vector3(0.05f, 0, 0);
            aimDownSights = true;
            player.GetMovementController.playerCamera.fieldOfView = 50;
            player.GetMovementController.aimMultiplier = 0.5f;
        }
        if (Input.GetMouseButtonUp(1)) {                    //when stopped aiming down sights.
            combinedIK.localPosition = new Vector3(0.3f, 0, 0);
            aimDownSights = false;
            player.GetMovementController.playerCamera.fieldOfView = 60;
            player.GetMovementController.aimMultiplier = 1f;
        }

        bool hitfire = false;

        if (Input.GetButtonDown(player.controlType.ToString() + " Fire")) { // When we start firing, we set the origin of the combined IK parent
            combinedIKoriginPos = combinedIK.localPosition;
        }

        if(player.controlType == Player.Controltype.Mouse) {
            hitfire = Input.GetButton(player.controlType.ToString() + " Fire");
        } else {
            hitfire = Input.GetAxis(player.controlType.ToString() + " Fire") > 0;
        }

        if (hitfire && currentWeapon != null && !isReloading && CanShoot()) {//When we are holding the mouse/button
            pressFire = hitfire;
            recoilCooldownMultiplier = 1;

            //All different shoot types
            if (currentWeapon.fireMode == Weapon.FireMode.Semi || currentWeapon.fireMode == Weapon.FireMode.ShotGun) {
                if (fireMode < 1) {//only shoot once for semi and shotgun, even when mouse button is held
                    Shoot();
                }
            }
            else if (currentWeapon.fireMode == Weapon.FireMode.Burst) {
                if (fireMode < 5) {//only shoot 5 round for burst, even when mouse button is held
                    Shoot();
                }
            }
            else if (currentWeapon.fireMode == Weapon.FireMode.Auto) { // auto will keep shooting based on the rate of fire
                Shoot();
            }
            else if (currentWeapon.fireMode == Weapon.FireMode.Flamethrower) { // needs to be different for flamethrower
                Shoot();
            }
            else if (currentWeapon.fireMode == Weapon.FireMode.Throwable) { // throwables need different code
                currentWeapon.gameObject.SetActive(false);

                GameObject go = Instantiate(currentWeapon.throwableObj);
                go.transform.position = currentWeapon.transform.position + player.GetMovementController.playerCamera.transform.forward * 0.1f;
                go.transform.rotation = currentWeapon.transform.rotation;

                go.transform.GetComponent<Rigidbody>().velocity = player.GetMovementController.playerCamera.transform.forward * 10;

                Destroy(currentWeapon.gameObject);
                currentWeapon = null;
                secondaryWeapon = null;
                leftIK.localPosition = leftIKorigin;
                return;
            }
            else if (currentWeapon.fireMode == Weapon.FireMode.Melee) { // melee needs different functions
                if (fireMode < 1) {
                    Melee();
                }
            }

            if(currentWeapon.ammo <= 0) {//When we are holding mouse, but ammo is empt
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

            shooting = true;
        }

        if (pressFire && hitfire != pressFire) { // mostly for flamethrower. Stop sounds and particle system
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

            shooting = false;
        }

        //disable muzzleflash after a few frames
        if (currentWeapon != null && currentWeapon.muzzleFlash != null && currentWeapon.muzzleFlash.activeSelf) {
            muzzleFlashShownFrames++;
            if (muzzleFlashShownFrames > 3) {
                currentWeapon.muzzle.GetComponent<Light>().enabled = false;
                currentWeapon.muzzleFlash.SetActive(false);
                muzzleFlashShownFrames = 0;
            }
        }
    }

    //check if we can aim down sights
    bool CanAimDownSights() {
        return currentWeapon != null && (currentWeapon.fireMode != Weapon.FireMode.Throwable || currentWeapon.fireMode != Weapon.FireMode.Melee) && !GetComponent<FirstPersonPlayerController>().sprinting;
    }

    //check if we are able to shoot
    bool CanShoot() {
        return currentWeapon.ammo > 0 && !GetComponent<FirstPersonPlayerController>().sprinting && Time.timeScale > 0;
    }

    //check if we are able to reload
    bool CanReload() {
        return currentWeapon.holdingmaxAmmo > 0 && currentWeapon.ammo < currentWeapon.maxAmmoMagazine;
    }

    //Picking up weapons mechanics
    public void PickUpGun(Weapon weapon) {
        //When we click a gun we already own, add ammo and delete weaponObject on the floor
        if (currentWeapon != null && currentWeapon.weaponName.Equals(weapon.weaponName)) {
            currentWeapon.holdingmaxAmmo += weapon.holdingmaxAmmo + weapon.ammo;
            UpdateAmmoCounter();
            Destroy(weapon.gameObject);
            return;
        }
        else if (secondaryWeapon != null && secondaryWeapon.weaponName.Equals(weapon.weaponName)) {
            secondaryWeapon.holdingmaxAmmo += weapon.holdingmaxAmmo + weapon.ammo;
            Destroy(weapon.gameObject);
            return;
        } 
        else if (primaryWeapon != null && primaryWeapon.weaponName.Equals(weapon.weaponName)) {
            primaryWeapon.holdingmaxAmmo += weapon.holdingmaxAmmo + weapon.ammo;
            Destroy(weapon.gameObject);
            return;
        }

        if (weapon.preveredSlot == WeaponSlot.primary) {// when prevered slot for weapon is primary
            if (primaryWeapon == null) {//if we dont have a primary weapon, add it
                primaryWeapon = weapon;
                if (currentWeapon != null)
                    currentWeapon.gameObject.SetActive(false);

                currentWeapon = primaryWeapon;
                currentWeapon.interactable = false;
                currentSlot = WeaponSlot.primary;
                UpdateAmmoCounter();

                WeaponFromFloorToHand(weapon);

                ResetReload();
            } else {            //if we already have a primary weapon, swap it and throw the other on the floor
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
        } else if (weapon.preveredSlot == WeaponSlot.secondary) {// when prevered slot for weapon is secondary
            if (secondaryWeapon == null) {//if we dont have a secondary weapon, add it
                secondaryWeapon = weapon;
                if (currentWeapon != null)
                    currentWeapon.gameObject.SetActive(false);

                currentWeapon = secondaryWeapon;
                currentSlot = WeaponSlot.secondary;
                currentWeapon.interactable = false;
                UpdateAmmoCounter();

                WeaponFromFloorToHand(weapon);

                ResetReload();
            } else {            //if we already have a secondary weapon, swap it and throw the other on the floor
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

    //What happens when a weapon in picked up
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

    //What happens on reload
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

    //What happens when Reload is done
    void ResetReload() {
        reloadTimer = 0;
        isReloading = false;
        hand.localEulerAngles = new Vector3(0, 0, 90);
        rightIK.localEulerAngles = rightIKoriginRot;
        rightIK.localPosition = rightIKoriginPos;
        if (currentWeapon.secondHand) {
            leftIK.position = currentWeapon.secondHand.position;
            leftIK.rotation = currentWeapon.secondHand.rotation;
        }else {
            leftIK.localPosition = leftIKorigin;
        }
    }

    //Add recoil to guns, also animate the IK to go backwards
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

    //simple melee mechanics
    void Melee() {
        if (rateOfFire > 0)
            return;

        fireMode++;

        Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 2)) {
            Hit(hit, true, true);
        }

        StartCoroutine(MeleeAnimation());

        //add to rate of fire, to make sure melee can't be used every frame
        if (currentWeapon.rateOfFire > 0) {
            rateOfFire += 60.0f / currentWeapon.rateOfFire;
        }
    }

    //simple melee animation
    IEnumerator MeleeAnimation() {
        float timer = 0.5f;
        while (timer > 0) {
            timer -= Time.deltaTime;
            rightIK.localPosition = rightIKoriginPos + new Vector3(0, 0, meleeAnimationCurve.Evaluate(timer / 0.5f) * 0.2f);
            rightIK.localEulerAngles = rightIKoriginRot + new Vector3(meleeAnimationCurve.Evaluate(timer / 0.5f) * 30f, 0, 0);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    //Gun mechanics
    void Shoot() {
        if (rateOfFire > 0)
            return;

        if (!CanShoot())
            return;

        fireMode++;

        Vector3 recoilOffset = new Vector3((Random.value - 0.5f) * recoilFactor * 0.15f, (Random.value - 0.5f) * recoilFactor * 0.15f, (Random.value - 0.5f) * recoilFactor * 0.15f);  

        if (currentWeapon.fireMode == Weapon.FireMode.ShotGun) { //shotgun spawns 5 rays, in a random patern
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
        else if (currentWeapon.fireMode == Weapon.FireMode.Flamethrower) {//flamethrower only needs sounds to start
            currentWeapon.muzzle.GetChild(0).GetComponent<ParticleSystem>().Play();
            Sound();
        }
        else {// all other guns, just shoot
            Ray ray = new Ray(player.GetMovementController.playerCamera.transform.position, player.GetMovementController.playerCamera.transform.forward + recoilOffset);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                Hit(hit, true, true);
            }
            MuzzleFlash();
            SpawnShell();
            Sound();
        }

        //add time to rate of fire.
        if (currentWeapon.rateOfFire > 0) {
            rateOfFire += 60.0f / currentWeapon.rateOfFire;
        }

        currentWeapon.ammo--;

        UpdateAmmoCounter();

        Recoil();
    }

    //Play a sound, if supported
    void Sound() {
        if (currentWeapon.begin == null && currentWeapon.end == null) {
            GameObject shotSoundobj = Instantiate(soundObj, currentWeapon.muzzle.position, Quaternion.identity);
            shotSoundobj.transform.SetParent(currentWeapon.muzzle);
            shotSoundobj.transform.localPosition = Vector3.zero;

            AudioSource source = shotSoundobj.GetComponent<AudioSource>();

            source.outputAudioMixerGroup = effectsMixerGroup;
            source.clip = currentWeapon.shoot;
            source.Play();
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

    //What happens when you are hit
    void Hit(RaycastHit hit, bool decal, bool hurt) {
        if (hit.collider.GetComponent<Barrel>()) {
            hit.collider.GetComponent<Barrel>().health--;
            if (hit.collider.GetComponent<Barrel>().health <= 0) {
                hit.collider.GetComponent<Barrel>().Explode();
                return;
            }
        }

        if(hurt) // can be false for flamethrower
            Hurt(hit, decal);
    }

    //Add a muzzleflash, toggle it and rotate for randomness
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

    //Hurt enemy
    void Hurt(RaycastHit hit, bool decal) {
        if (!hit.collider.GetComponent<EnemyPart>()) {
            if (decal)
                Decal(hit);

            return;
        }

        EnemyPart part = hit.collider.GetComponent<EnemyPart>();
        part.Damage(currentWeapon.damageDone, transform);
    }

    //Add a decal to an object when hit
    void Decal(RaycastHit hit) {
        if (hit.collider.isTrigger)
            return;

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
            decalList.Add(go);
        }
    }

    //spawns a shell next to the gun, if it has a shell
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

    //Update the Ammo counter
    public void UpdateAmmoCounter() {
        ammoText.text = "" + currentWeapon.ammo + "/" + currentWeapon.holdingmaxAmmo + "";
    }

    //When you swap slots
    void SwapWeapon(WeaponSlot slot) {
        if (currentSlot == slot)
            return;

        if (slot == WeaponSlot.primary && primaryWeapon != null) {
            currentSlot = slot;
            DrawWeapon(primaryWeapon);
        }

        if (slot == WeaponSlot.secondary && secondaryWeapon != null) {
            currentSlot = slot;
            DrawWeapon(secondaryWeapon);
        }

        ResetReload();
        UpdateAmmoCounter();
    }

    //When you change your main weapon to an other weapon
    void DrawWeapon(Weapon weapon) {
        if(currentWeapon != null)
            currentWeapon.gameObject.SetActive(false);

        currentWeapon = weapon;
        currentWeapon.gameObject.SetActive(true);
    }
}
