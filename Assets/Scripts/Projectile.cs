using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public ProjectileObject projectileData;
    private float despawnTimer;

    private void Start() {
        despawnTimer = 3;
    }

    private void Update() {
        despawnTimer -= Time.deltaTime;

        if (despawnTimer <= 0) {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {

        if (collision.gameObject.CompareTag("Enemy")) {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            enemy.TakeDamage(projectileData.damage);
            Destroy(gameObject);
        }
    }

}
