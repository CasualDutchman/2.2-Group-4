using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Props/Weapon", order = 1)]
public class Weapon : ScriptableObject {

    public enum FireMode { Semi, Burst, Auto }

    public FireMode fireMode;
    public float rateOfFire;
    public int maxAmmo;

	void Start () {
		
	}
	
	void Update () {
		
	}
}
