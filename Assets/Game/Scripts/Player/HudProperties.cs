using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class HudProperties : MonoBehaviour {

    //All the properties of the HUD, this way the HUD objects can be attached to the players variables

    public Transform p1HealthImage, p1AmmoText, p1Crosshair, p1PickupText, p1RadImage, p1StaminaImage;
    public GameObject p1DeathRetry, p1DeathExit, p1ButtonExit, p1ButtonRetry, p1ButtonExit2;
    public Transform p2HealthImage, p2AmmoText, p2Crosshair, p2PickupText, p2RadImage, p2StaminaImage;
}
