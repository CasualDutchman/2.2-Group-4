using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScript : MonoBehaviour {

    public static DemoScript instance;

    public float healthMultiplier = 1;
    public float damageMultiplier = 1;
    public float enemyDamageMultiplier = 1;
    public float speed = 3.5f;

	void Awake () {
        instance = this;

        GetComponent<GridWorld>().maxEnemySpawned = PlayerPrefs.GetInt("EnemySpawned");
        damageMultiplier = PlayerPrefs.GetFloat("Multi");
        enemyDamageMultiplier = PlayerPrefs.GetFloat("EnemyMulti");
        speed = PlayerPrefs.GetFloat("Speedy");

        healthMultiplier = PlayerPrefs.GetFloat("HPMulti");

    }
}
