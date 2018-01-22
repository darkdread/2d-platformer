using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public ProjectileObject projectileData;

    public static List<GameObject> list = new List<GameObject>();
    private static float despawnTimerInitial = 5;

    private List<string> enemyList = new List<string>();

    private Rigidbody2D rb;
    private float despawnTimer;
    private bool collided;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        despawnTimer = despawnTimerInitial;

        list.Add(gameObject);
    }

    private void Update() {
        if (Game.paused || Main.EditorMode) return;

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

    public void Reflect() {
        for(int i = enemyList.Count - 1; i >= 0; i--) {
            string tag = enemyList[i];

            if (tag == "Enemy") {
                AddEnemyTag("Player");
            } else if (tag == "Player") {
                AddEnemyTag("Enemy");
            }

            RemoveEnemyTag(tag);
        }

        despawnTimer = despawnTimerInitial;

        Vector2 moveVel = new Vector2(-rb.velocity.x, -rb.velocity.y);
        //moveVel = Vector2.Reflect(rb.velocity, Vector2.left);

        transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        rb.velocity = moveVel;
    }

    public bool IsEnemyOf(string tag) {
        
        if (enemyList.Contains(tag)) {
            return true;
        }

        return false;
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

    private void OnTriggerStay2D(Collider2D collision) {
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

                    Vector2 knockbackForce;
                    if (transform.localScale.z == 0) {
                        
                        float directionX = (transform.localScale.x > 0) ? 1 : -1;
                        knockbackForce = new Vector2(projectileData.knockbackForce * directionX, Mathf.Abs(projectileData.knockbackForce));
                    } else {
                        // Use transform.right to knock player towards facing of projectile
                        // Use transform.up to knock player towards direction of projectile

                        knockbackForce = transform.up * projectileData.knockbackForce;
                    }

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
