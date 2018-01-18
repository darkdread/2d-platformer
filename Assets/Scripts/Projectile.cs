using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public ProjectileObject projectileData;
    public static List<GameObject> list = new List<GameObject>();
    private List<string> enemyList = new List<string>();

    private Rigidbody2D rb;
    private float despawnTimer;
    private bool collided;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        despawnTimer = 3;

        list.Add(gameObject);
    }

    private void Update() {
        if (Game.paused) return;

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
        }
    }

    public void RemoveEnemyTag(string tag) {
        if (enemyList.Contains(tag)) {
            enemyList.Remove(tag);
        }
    }

    private void OnDestroy() {
        list.Remove(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collided) {
            return;
        }

        foreach (string tag in enemyList) {
            if (collision.gameObject.CompareTag(tag)) {
                collided = true;

                Enemy enemy = collision.GetComponent<Enemy>();
                Player player = collision.GetComponent<Player>();
                IDamageableObject damageableObject = collision.GetComponent<IDamageableObject>();

                if (damageableObject != null) {
                    damageableObject.TakeDamage(projectileData.damage);
                    float directionX = (transform.localScale.x > 0) ? 1 : -1;
                    Vector2 knockbackForce = new Vector2(projectileData.knockbackForce * directionX, Mathf.Abs(projectileData.knockbackForce));
                    damageableObject.Knockback(knockbackForce);
                }

                /*if (enemy) {
                    enemy.TakeDamage(projectileData.damage);
                    float directionX = (transform.localScale.x > 0) ? 1 : -1;
                    Vector2 knockbackForce = new Vector2(projectileData.knockbackForce * directionX, Mathf.Abs(projectileData.knockbackForce));
                    enemy.Knockback(knockbackForce);
                } else if (player) {
                    player.TakeDamage(projectileData.damage);
                    float directionX = (transform.localScale.x > 0) ? 1 : -1;
                    Vector2 knockbackForce = new Vector2(projectileData.knockbackForce * directionX, Mathf.Abs(projectileData.knockbackForce));
                    player.Knockback(knockbackForce);
                }*/

                Destroy(gameObject);
            }
        }
    }

}
