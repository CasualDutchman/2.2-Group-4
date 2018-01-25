using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class DemoScript : MonoBehaviour {

    //This script was used for testing.
    //It allows to change the multipliers in playerPrefs and load them when playing.
    //This way we could test multiple games and check what the tested liked better

    //Instance, so all scripts can call from it, without a reference
    public static DemoScript instance;

    public float healthMultiplier = 1;
    public float damageMultiplier = 1;
    public float enemyDamageMultiplier = 1;
    public float speed = 3.5f;

    //Normal gamemode will be false, testing true.
    public bool usePlayerPref = true;

	void Awake () {
        instance = this;

        //Set all the multipliers to the multipliers specified in the mainmenu class
        if (usePlayerPref) {
            if (GetComponent<GridWorld>())
                GetComponent<GridWorld>().maxEnemySpawned = PlayerPrefs.GetInt("EnemySpawned");

            damageMultiplier = PlayerPrefs.GetFloat("Multi");
            enemyDamageMultiplier = PlayerPrefs.GetFloat("EnemyMulti");
            speed = PlayerPrefs.GetFloat("Speedy");

            healthMultiplier = PlayerPrefs.GetFloat("HPMulti");
        }
    }
}
