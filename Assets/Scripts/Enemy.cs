using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageableObject {

    public static List<GameObject> list = new List<GameObject>();
    public EnemyObject enemyData;
    public Transform groundCheckLeft, groundCheckRight;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private float health;
    private float movementTimer;
    private float movementCheckDelayTimer = 1f;
    private float lastEnemyX;
    private float attackTimer;

    // How long the knockback lasts
    public float knockbackLength = 0.5f;
    // The amount of time to trigger knockback again
    private float knockbackTimer;

    // Use this for initialization
    private void Start () {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        health = enemyData.health;
        attackTimer = Random.Range(enemyData.attackDelayMin, enemyData.attackDelayMax);
        movementTimer = movementCheckDelayTimer;
        lastEnemyX = transform.position.x;

        list.Add(gameObject);
	}
	
	// Update is called once per frame
	private void Update () {
        if (Main.EditorMode || Game.paused) {
            return;
        }

        bool isGrounded = false;
        // Check if the enemy is on the ground.
        Collider2D[] result = (transform.localScale.x > 0) ? Physics2D.OverlapCircleAll(groundCheckRight.position, enemyData.groundCheckRadius, enemyData.realGround) : Physics2D.OverlapCircleAll(groundCheckLeft.position, enemyData.groundCheckRadius, enemyData.realGround);

        // Make sure we aren't checking for the enemy itself
        foreach (Collider2D collider in result) {
            if (collider.gameObject.GetInstanceID() != this.gameObject.GetInstanceID()) {
                isGrounded = true;
                goto End;
            } else {
                isGrounded = false;
            }
        }
        End:;

        // Size of the box is split in half, as the box is positioned on the enemy.
        // So to check for the player from 5 world units distance away, the box is 5 x 2 units big.

        Vector2 size = new Vector2(10, 2);
        bool isPlayerNear = IsPlayerInBox(size);
        if (isPlayerNear && attackTimer <= 0) {
            
        }

        if (knockbackTimer <= 0) {
            attackTimer -= Time.deltaTime;

            if (spriteRenderer.color == Color.red) {
                spriteRenderer.color = Color.white;
            }

            lastEnemyX = transform.position.x - rb.velocity.x;

            if (isPlayerNear) {
                Transform player = FindObjectOfType<Player>().transform;
                float faceLeft = (transform.position.x > player.position.x) ? -1 : 1;
                transform.localScale = new Vector3(faceLeft, transform.localScale.y, transform.localScale.z);

                if (attackTimer <= 0) {
                    Projectile projectile = LevelController.CreateProjectileTowardsDirection(Game.current.ProjectileDictionary["kunai"], transform.position + transform.localScale.x * Vector3.right * 0.5f, transform.position + transform.localScale.x * Vector3.right * 2);
                    LevelController.SetProjectileEnemyAgainst(projectile, "Player");
                    attackTimer = Random.Range(enemyData.attackDelayMin, enemyData.attackDelayMax);
                }
            }

            float moveLeft = (transform.localScale.x < 0) ? -1 : 1;

            rb.velocity = new Vector2(enemyData.moveSpeed * moveLeft, rb.velocity.y);
            movementTimer -= Time.deltaTime;

            //print(string.Format("curPos: {0}, lastPos: {1}", rb.position.x, lastEnemyX));

            if (!isGrounded && movementTimer <= 0) {
                // If enemy is not grounded, flip its scale and position it in a way so that on the next frame, the enemy
                // will be back on the ground. (In this case, it is the movement speed of the enemy * 2 world units.)
                movementTimer = movementCheckDelayTimer;

                transform.localScale = new Vector3(moveLeft * -1, transform.localScale.y, transform.localScale.z);
                transform.position = new Vector2(transform.position.x + (enemyData.moveSpeed * (moveLeft * -1) * Time.deltaTime * 2), transform.position.y);
            } else if (isGrounded && movementTimer <= 0 && lastEnemyX == rb.position.x) {
                // If the enemy is on the ground, but it hasn't moved since the previous physics update, flip the enemy
                // and reset the movement timer so that it will begin checking again.
                movementTimer = movementCheckDelayTimer;

                transform.localScale = new Vector3(moveLeft * -1, transform.localScale.y, transform.localScale.z);
            }
        } else {
            knockbackTimer -= Time.deltaTime;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        
        //Gizmos.DrawCube(transform.position, new Vector3(10, 2, 1));
    }

    // Detect if a player is in box of size XY
    public bool IsPlayerInBox(Vector2 size) {
        Collider2D isPlayerInRange = Physics2D.OverlapBox(transform.position, size, 0, 1 << LayerMask.NameToLayer("Player"));

        if (isPlayerInRange) {
            if (isPlayerInRange.CompareTag("Player")) {
                return true;
            }
        }
        return false;
    }

    // Allows other classes to knockback the enemy
    public void Knockback(Vector2 force) {

        // if the enemy is not already getting knocked
        if (knockbackTimer <= 0) {
            //set the length of the knockback
            knockbackTimer = knockbackLength;

            spriteRenderer.color = Color.red;
            rb.velocity = force;
        }
    }

    private void OnDestroy() {
        list.Remove(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Enemy")) {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
        }
    }

    public void TakeDamage(float damage) {
        health -= damage;

        if (health <= 0) {
            Destroy(gameObject);
        }
    }
}
