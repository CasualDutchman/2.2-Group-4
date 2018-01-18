using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScript : MonoBehaviour {

    public static DemoScript instance;

    public float healthMultiplier;
    public float damageMultiplier;
    public float enemyDamageMultiplier;
    public float speed;

	void Awake () {
        instance = this;

        GetComponent<GridWorld>().maxEnemySpawned = PlayerPrefs.GetInt("EnemySpawned");
        damageMultiplier = PlayerPrefs.GetFloat("Multi");
        enemyDamageMultiplier = PlayerPrefs.GetFloat("EnemyMulti");
        speed = PlayerPrefs.GetFloat("Speedy");

        healthMultiplier = PlayerPrefs.GetFloat("HPMulti");

    }
}
