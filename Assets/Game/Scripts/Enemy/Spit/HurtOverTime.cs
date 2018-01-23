using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtOverTime : MonoBehaviour {

    public enum Selection { Both, Enemy, Player };

    public Selection enemyType;

    public float damage = 2;
    public float radDamage = 3;
    public float interval = 1;
    public float EnemySlowSpeed = 1.0f;

    public bool canMakeExplode = true;

    float timer;

    void OnTriggerStay(Collider col) {
        timer += Time.fixedDeltaTime;
        if (timer >= interval) {
            timer -= interval;

            if (enemyType == Selection.Player || enemyType == Selection.Both) {
                if (col.CompareTag("Player")) {
                    if (col.GetComponent<Player>()) {
                        Player player = col.GetComponent<Player>();
                        player.Hurt(damage);
                        player.Radiate(radDamage);
                    }
                }
            }
            if (enemyType == Selection.Enemy || enemyType == Selection.Both) {
                if (col.CompareTag("Enemy")) {
                    if (col.GetComponent<EnemyPart>()) {
                        EnemyPart enemy = col.GetComponent<EnemyPart>();
                        //enemy.Damage(5, null);
                        enemy.connected.Hurt(1);
                        if (gameObject.CompareTag("Acid")) {
                            enemy.connected.GetAgent().speed = EnemySlowSpeed;
                        }
                    }
                }
            }

            if (canMakeExplode) {
                if (col.GetComponent<Barrel>()) {
                    Barrel barrel = col.GetComponent<Barrel>();
                    barrel.health -= 0.5f;
                    if (barrel.health <= 0) {
                        barrel.Explode();
                    }
                }

                if (col.GetComponent<Weapon>() && col.GetComponent<Weapon>().fireMode == Weapon.FireMode.Throwable && col.GetComponent<Weapon>().weaponName.ToLower().StartsWith("molo")) {
                    Weapon weapon = col.GetComponent<Weapon>();
                    GameObject go = Instantiate(weapon.throwableObj);
                    go.transform.position = col.transform.position;

                    Destroy(col.gameObject);
                }
            }
        }
    }

    void OnTriggerExit(Collider col) {
        if (enemyType == Selection.Enemy || enemyType == Selection.Both) {
            if (col.CompareTag("Enemy")) {
                if (col.GetComponent<EnemyPart>()) {
                    EnemyPart enemy = col.GetComponent<EnemyPart>();
                    if (gameObject.CompareTag("Acid")) {
                        enemy.connected.GetAgent().speed = DemoScript.instance.speed;
                    }
                }
            }
        }
    }
}
