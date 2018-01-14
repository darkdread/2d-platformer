using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public ProjectileObject projectileData;
    private List<string> enemyList = new List<string>();

    private Rigidbody2D rb;
    private float despawnTimer;
    private bool collided;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        despawnTimer = 3;
    }

    private void Update() {
        despawnTimer -= Time.deltaTime;

        // Future rotation code
        /*var dir = rb.velocity;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 500 * Time.deltaTime);*/

        if (despawnTimer <= 0) {
            Destroy(gameObject);
        }
    }

    public void AddEnemyTag(string tag) {
        if (!enemyList.Contains(tag)) {
            enemyList.Add(tag);
            print("added" + tag);
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collided) {
            return;
        }

        foreach (string tag in enemyList) {
            if (collision.gameObject.CompareTag(tag)) {
                collided = true;

                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                Player player = collision.gameObject.GetComponent<Player>();

                if (enemy) {
                    enemy.TakeDamage(projectileData.damage);
                    float directionX = (transform.localScale.x > 0) ? 1 : -1;
                    Vector2 knockbackForce = new Vector2(projectileData.knockbackForce * directionX, Mathf.Abs(projectileData.knockbackForce));
                    enemy.Knockback(knockbackForce);
                } else if (player) {
                    player.TakeDamage(projectileData.damage);
                    float directionX = (transform.localScale.x > 0) ? 1 : -1;
                    Vector2 knockbackForce = new Vector2(projectileData.knockbackForce * directionX, Mathf.Abs(projectileData.knockbackForce));
                    player.Knockback(knockbackForce);
                }

                Destroy(gameObject);
            }
        }
    }

}
