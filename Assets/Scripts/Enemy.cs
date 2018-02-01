using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageableObject {

    public static List<GameObject> list = new List<GameObject>();
    public EnemyObject enemyData;
    public Transform groundCheckLeft, groundCheckRight;

    private Dictionary<string, GameObject> colliders = new Dictionary<string, GameObject>();
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float health;
    private float movementTimer;
    private float movementCheckDelayTimer = 0.5f;
    private float lastEnemyX;

    // The difference between skillTimer and skillCooldownTimer is that the skillTimer resets on each skill cast
    // While skillCooldownTimer resets on each specific skill cast. For example, if our enemy has a
    // ThrowKunai and MeleeAttack skill, when the kunai is thrown, the kunai's timer and skill timer resets. But
    // the MeleeAttack skill timer is not. Each skillCooldownTimer is independent of its own, while skillTimer is shared
    // amont all skills.

    private float skillTimer;
    private List<float> skillCooldownTimer = new List<float>();

    private bool isBlocking, isPlayerInVisionRange, isPlayerNear;

    // How long the knockback lasts
    public float knockbackLength = 0.5f;
    // The amount of time to trigger knockback again
    private float knockbackTimer;

    public bool IsBlocking {
        get {
            return isBlocking;
        }

        set {
            isBlocking = value;
        }
    }

    // Use this for initialization
    private void Start () {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        health = enemyData.health;
        skillTimer = Random.Range(enemyData.skillDelayMin, enemyData.skillDelayMax);
        movementTimer = movementCheckDelayTimer;
        lastEnemyX = transform.position.x;

        foreach (EnemySkillObject skill in enemyData.skills) {
            skillCooldownTimer.Add(skill.attackDelayMax);
        }

        if (transform.Find("Colliders")) {
            BoxCollider2D[] boxColliders = transform.Find("Colliders").GetComponentsInChildren<BoxCollider2D>();

            foreach (BoxCollider2D collider in boxColliders) {
                colliders[collider.name] = collider.gameObject;
                collider.gameObject.SetActive(false);
            }
        }

        list.Add(gameObject);
	}
	
	// Update is called once per frame
	private void Update () {
        if (Main.EditorMode || Game.paused) {
            return;
        }

        bool isGrounded = false;
        // Check if the enemy is on the ground.
        //Collider2D[] result = (transform.localScale.x > 0) ? Physics2D.OverlapCircleAll(groundCheckRight.position, enemyData.groundCheckRadius, enemyData.realGround) : Physics2D.OverlapCircleAll(groundCheckLeft.position, enemyData.groundCheckRadius, enemyData.realGround);
        Collider2D[] result = Physics2D.OverlapCircleAll(groundCheckRight.position, enemyData.groundCheckRadius, enemyData.realGround);

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
        isPlayerInVisionRange = IsPlayerInBox(size);

        if (knockbackTimer <= 0) {
            skillTimer -= Time.deltaTime;
            for (int i = 0; i < skillCooldownTimer.Count; i++) {
                if (skillCooldownTimer[i] > 0) {
                    // Reset timer
                    skillCooldownTimer[i] -= Time.deltaTime;
                }
            }

            if (spriteRenderer.color == Color.red) {
                spriteRenderer.color = Color.white;
            }

            lastEnemyX = transform.position.x - rb.velocity.x;

            if (isPlayerInVisionRange) {
                Transform player = FindObjectOfType<Player>().transform;
                float faceLeft = (transform.position.x > player.position.x) ? -1 : 1;
                transform.localScale = new Vector3(faceLeft, transform.localScale.y, transform.localScale.z);

                if (skillTimer <= 0) {
                    for(int i = 0; i < skillCooldownTimer.Count; i++) {
                        if (skillCooldownTimer[i] <= 0) {
                            size = new Vector2(3, 3);
                            isPlayerNear = IsPlayerInBox(size);

                            // Cast spell
                            Cast(enemyData.skills[i]);

                            // Reset timer
                            skillCooldownTimer[i] = Random.Range(enemyData.skills[i].attackDelayMin, enemyData.skills[i].attackDelayMax);
                        }
                    }
                    
                    skillTimer = Random.Range(enemyData.skillDelayMin, enemyData.skillDelayMax);
                }
            } else {
                movementTimer -= Time.deltaTime;
            }

            float moveLeft = (transform.localScale.x < 0) ? -1 : 1;

            rb.velocity = new Vector2(enemyData.moveSpeed * moveLeft, rb.velocity.y);

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
            animator.SetBool("Knockbacked", false);

        } else {
            knockbackTimer -= Time.deltaTime;
            animator.SetBool("Knockbacked", true);
        }
    }

    private void Cast(EnemySkillObject skill) {
        switch (skill.id) {
            case 0:
                Projectile projectile = LevelController.CreateProjectileTowardsDirection(Game.current.ProjectileDictionary["kunai"], transform.position + transform.localScale.x * Vector3.right * 0.5f, transform.position + transform.localScale.x * Vector3.right * 2);
                LevelController.SetProjectileEnemyAgainst(projectile, "Player");
                projectile.damage = skill.damage;
                break;
            case 1:
                bool isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
                if (!isAttacking && isPlayerNear) {
                    animator.SetInteger("RandAttack", 0);
                    animator.SetTrigger("EnemyAttack");
                }
                break;
        }
    }

    private void DisableCollider(string name) {
        GameObject obj = colliders.TryGetValue(name, out obj) ? obj : null;
        if (obj) {
            obj.SetActive(false);
        } else {
            Debug.Log(string.Format("{0} is not found in collider dictionary!", name));
        }
    }

    private void EnableCollider(string name) {
        GameObject obj = colliders.TryGetValue(name, out obj) ? obj : null;
        if (obj) {
            obj.SetActive(true);
        } else {
            Debug.Log(string.Format("{0} is not found in collider dictionary!", name));
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        
        //Gizmos.DrawCube(transform.position, new Vector3(3, 3, 1));
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
            animator.SetBool("Knockbacked", true);
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
