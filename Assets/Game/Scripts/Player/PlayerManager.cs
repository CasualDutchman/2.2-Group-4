using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

    public bool onAwake = false;

    public Player.playertype type;

    public Player.Controltype playerOneControls;
    public Player.Controltype playerTwoControls;

    public GameObject playerprefab;
    public GameObject onePlayerHudPrefab;
    public GameObject twoPlayerHudPrefab;

    public Transform canvas;

    public Transform playerOneSpawn, playerTwoSpawn;

	void Start () {
        if (onAwake)
            Play();
    }

    public void Play() {
        type = PlayerPrefs.GetInt("PlayerCount") == 2 ? Player.playertype.PlayerTwo : Player.playertype.PlayerOne;

        if (type == Player.playertype.PlayerTwo && playerOneControls == playerTwoControls) {
            Debug.LogError("Players controls are the same");
        }

        HudProperties properties;
        GameObject playerOneObj = Instantiate(playerprefab, playerOneSpawn.position, playerOneSpawn.rotation);
        GameObject playerTwoObj;

        playerOneObj.name = "Player 1";

        Camera cameraOne = playerOneObj.transform.GetChild(0).GetComponent<Camera>();
        playerOneObj.GetComponent<FirstPersonPlayerController>().playerCamera = cameraOne;

        if (type == Player.playertype.PlayerTwo) {
            GameObject hud = Instantiate(twoPlayerHudPrefab, canvas);
            properties = hud.GetComponent<HudProperties>();

            playerTwoObj = Instantiate(playerprefab, playerTwoSpawn.position, playerTwoSpawn.rotation);
            playerTwoObj.name = "Player 2";

            Player playerTwo = playerTwoObj.GetComponent<Player>();
            playerTwo.healthText = properties.p2HealthText.GetComponent<Text>();
            playerTwo.playerType = Player.playertype.PlayerTwo;
            playerTwo.controlType = playerTwoControls;

            PlayerWeaponController weaponControllerTwo = playerTwoObj.GetComponent<PlayerWeaponController>();
            weaponControllerTwo.pickUpText = properties.p2PickupText.GetComponent<Text>();
            weaponControllerTwo.ammoText = properties.p2AmmoText.GetComponent<Text>();
            weaponControllerTwo.recoilCrosshair = properties.p2Crosshair.GetComponent<RectTransform>();

            Camera cameraTwo = playerTwoObj.transform.GetChild(0).GetComponent<Camera>();
            playerTwoObj.GetComponent<FirstPersonPlayerController>().playerCamera = cameraTwo;

            cameraOne.rect = new Rect(0, 0.5f, 1, 0.5f);
            cameraTwo.rect = new Rect(0, 0, 1, 0.5f);
        } else {
            GameObject hud = Instantiate(onePlayerHudPrefab, canvas);
            properties = hud.GetComponent<HudProperties>();
        }

        Player playerOne = playerOneObj.GetComponent<Player>();
        playerOne.healthText = properties.p1HealthText.GetComponent<Text>();
        playerOne.playerType = Player.playertype.PlayerOne;
        playerOne.controlType = playerOneControls;

        PlayerWeaponController weaponControllerOne = playerOneObj.GetComponent<PlayerWeaponController>();
        weaponControllerOne.pickUpText = properties.p1PickupText.GetComponent<Text>();
        weaponControllerOne.ammoText = properties.p1AmmoText.GetComponent<Text>();
        weaponControllerOne.recoilCrosshair = properties.p1Crosshair.GetComponent<RectTransform>();


        Destroy(gameObject);
    }
}
