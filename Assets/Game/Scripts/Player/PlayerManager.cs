using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Author: Pieter
public class PlayerManager : MonoBehaviour {

    //normally controlled by gridWorld when the world is done generating.
    //When true, player spawns on awake
    public bool onAwake = false;

    public Player.playertype type; //how many players?

    //controls for every player
    public Player.Controltype playerOneControls;
    public Player.Controltype playerTwoControls;

    //spawnable player objects
    public GameObject playerprefab;
    public GameObject onePlayerHudPrefab;
    public GameObject twoPlayerHudPrefab;

    //canvas to be the parent
    public Transform canvas;

    //spawn location for the player
    public Transform playerOneSpawn, playerTwoSpawn;

	void Start () {
        if (onAwake)
            Play();
    }

    public void Play() {
        if (!onAwake) //Get the amount of players from the Playerprefs
            type = PlayerPrefs.GetInt("PlayerCount") == 2 ? Player.playertype.PlayerTwo : Player.playertype.PlayerOne;

        if (type == Player.playertype.PlayerTwo && playerOneControls == playerTwoControls) {
            Debug.LogError("Players controls are the same");
        }

        //What this does:
        //It spawns 1 or 2 players, as specified.
        //It will attach all the Hud elements accordingly, so those classes can change the elements when they need to

        HudProperties properties;
        GameObject playerOneObj = Instantiate(playerprefab, playerOneSpawn.position, playerOneSpawn.rotation);
        GameObject playerTwoObj;

        GameObject hud;

        playerOneObj.name = "Player 1";

        Camera cameraOne = playerOneObj.transform.GetChild(0).GetComponent<Camera>();
        FirstPersonPlayerController firstPerson = playerOneObj.GetComponent<FirstPersonPlayerController>();
        firstPerson.playerCamera = cameraOne;

        if (type == Player.playertype.PlayerTwo) {
            hud = Instantiate(twoPlayerHudPrefab, canvas);
            properties = hud.GetComponent<HudProperties>();

            playerTwoObj = Instantiate(playerprefab, playerTwoSpawn.position, playerTwoSpawn.rotation);
            playerTwoObj.name = "Player 2";

            Player playerTwo = playerTwoObj.GetComponent<Player>();
            playerTwo.healthImage = properties.p2HealthImage.GetComponent<Image>();
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
            hud = Instantiate(onePlayerHudPrefab, canvas);
            properties = hud.GetComponent<HudProperties>();
        }

        firstPerson.staminaImage = properties.p1StaminaImage.GetComponent<Image>();

        Player playerOne = playerOneObj.GetComponent<Player>();
        playerOne.healthImage = properties.p1HealthImage.GetComponent<Image>();
        playerOne.radiationImage = properties.p1RadImage.GetComponent<Image>();
        playerOne.playerType = Player.playertype.PlayerOne;
        playerOne.controlType = playerOneControls;
        playerOne.hudElement = hud;
        playerOne.deathRetry = properties.p1DeathRetry;
        playerOne.deathExit = properties.p1DeathExit;
        playerOne.buttonRetry = properties.p1ButtonRetry.GetComponent<Button>();
        playerOne.buttonExit = properties.p1ButtonExit.GetComponent<Button>();
        playerOne.buttonExit2 = properties.p1ButtonExit2.GetComponent<Button>();

        PlayerWeaponController weaponControllerOne = playerOneObj.GetComponent<PlayerWeaponController>();
        weaponControllerOne.pickUpText = properties.p1PickupText.GetComponent<Text>();
        weaponControllerOne.ammoText = properties.p1AmmoText.GetComponent<Text>();
        weaponControllerOne.recoilCrosshair = properties.p1Crosshair.GetComponent<RectTransform>();
    }

    //When the player respawns, place a new player
    public void Respawn(Player player, int lives) {
        DestroyImmediate(player.hudElement);
        DestroyImmediate(player.gameObject);

        GameObject playerOneObj = Instantiate(playerprefab, playerOneSpawn.position, playerOneSpawn.rotation);

        GameObject hud = Instantiate(onePlayerHudPrefab, canvas);
        HudProperties properties = hud.GetComponent<HudProperties>();

        Camera cameraOne = playerOneObj.transform.GetChild(0).GetComponent<Camera>();
        FirstPersonPlayerController firstPerson = playerOneObj.GetComponent<FirstPersonPlayerController>();
        firstPerson.playerCamera = cameraOne;
        firstPerson.staminaImage = properties.p1StaminaImage.GetComponent<Image>();

        Player playerOne = playerOneObj.GetComponent<Player>();
        playerOne.healthImage = properties.p1HealthImage.GetComponent<Image>();
        playerOne.radiationImage = properties.p1RadImage.GetComponent<Image>();
        playerOne.playerType = Player.playertype.PlayerOne;
        playerOne.controlType = playerOneControls;
        playerOne.hudElement = hud;
        playerOne.deathRetry = properties.p1DeathRetry;
        playerOne.deathExit = properties.p1DeathExit;
        playerOne.buttonRetry = properties.p1ButtonRetry.GetComponent<Button>();
        playerOne.buttonExit = properties.p1ButtonExit.GetComponent<Button>();
        playerOne.buttonExit2 = properties.p1ButtonExit2.GetComponent<Button>();

        playerOne.lives = lives;

        PlayerWeaponController weaponControllerOne = playerOneObj.GetComponent<PlayerWeaponController>();
        weaponControllerOne.pickUpText = properties.p1PickupText.GetComponent<Text>();
        weaponControllerOne.ammoText = properties.p1AmmoText.GetComponent<Text>();
        weaponControllerOne.recoilCrosshair = properties.p1Crosshair.GetComponent<RectTransform>();
    }
}
