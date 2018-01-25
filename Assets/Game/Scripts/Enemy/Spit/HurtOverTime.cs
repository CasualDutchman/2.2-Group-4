using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class HurtOverTime : MonoBehaviour {

    //Which character to hurt
    public enum Selection { Both, Enemy, Player };

    public Selection enemyType;

    //The amound of damage done over time
    public float damage = 2;
    public float radDamage = 3;
    public float interval = 1;

    //Can explode barrels and motolov Cocktails
    public bool canMakeExplode = true;

    //Can slow down the melee enemy
    public bool slowDownEnemy = false;

    //Can set enemies on fire
    public bool setsOnFire = false;

    float timer;

    void OnTriggerEnter(Collider col) {
        if (slowDownEnemy) {                    //When the object can slow down enemies
            if (col.CompareTag("Enemy")) {
                if (col.GetComponent<EnemyPart>()) {
                    if (col.GetComponent<EnemyPart>().connected is MeleeAI) {
                        EnemyPart enemy = col.GetComponent<EnemyPart>();
                        enemy.connected.LowerSpeed(0.5f);
                    }
                }
            }
        }

        if (setsOnFire) {                       //when object can set enemies on fire
            if (col.CompareTag("Enemy")) {
                if (col.GetComponent<EnemyPart>()) {
                    Enemy enemy = col.GetComponent<EnemyPart>().connected;
                    enemy.SetFire();
                }
            }
        }
    }

    void OnTriggerStay(Collider col) {
        timer += Time.fixedDeltaTime;
        if (timer >= interval) {
            timer -= interval;

            if (enemyType == Selection.Player || enemyType == Selection.Both) { //hurt player 
                if (col.CompareTag("Player")) {
                    if (col.GetComponent<Player>()) {
                        Player player = col.GetComponent<Player>();
                        player.Hurt(damage);
                        player.Radiate(radDamage);
                    }
                }
            }
            if (enemyType == Selection.Enemy || enemyType == Selection.Both) { // hurt enemie
                if (col.CompareTag("Enemy")) {
                    if (col.GetComponent<EnemyPart>()) {
                        EnemyPart enemy = col.GetComponent<EnemyPart>();
                        enemy.Damage(5, null);
                    }
                }
            }

            if (canMakeExplode) {               //make barrel explode
                if (col.GetComponent<Barrel>()) {
                    Barrel barrel = col.GetComponent<Barrel>();
                    barrel.health -= 0.5f;
                    if (barrel.health <= 0) {
                        barrel.Explode();
                    }
                }

                //make a molotov cocktail explode
                if (col.GetComponent<Weapon>() && col.GetComponent<Weapon>().fireMode == Weapon.FireMode.Throwable && col.GetComponent<Weapon>().weaponName.ToLower().StartsWith("molo")) {
                    Weapon weapon = col.GetComponent<Weapon>();
                    GameObject go = Instantiate(weapon.throwableObj);
                    go.transform.position = col.transform.position;

                    Destroy(col.gameObject);
                }
            }
        }
    }
}
