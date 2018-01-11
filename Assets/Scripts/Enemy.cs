using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public EnemyObject enemyData;
    public Transform groundCheckLeft, groundCheckRight;

    private Rigidbody2D rb;
    private Animator anim;
    private float health;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        health = enemyData.health;
	}
	
	// Update is called once per frame
	void Update () {
        if (Main.EditorMode) {
            return;
        }

        bool isGrounded = false;
        // Check if the enemy is on the ground.
        foreach (LayerMask mask in enemyData.realGround) {
            Collider2D[] result = (transform.localScale.x > 0) ? Physics2D.OverlapCircleAll(groundCheckRight.position, enemyData.groundCheckRadius, mask) : Physics2D.OverlapCircleAll(groundCheckLeft.position, enemyData.groundCheckRadius, mask);

            // Make sure we aren't checking for the enemy itself
            foreach (Collider2D collider in result) {
                if (collider.gameObject.GetInstanceID() != this.gameObject.GetInstanceID()) {
                    isGrounded = true;
                    goto End;
                } else {
                    isGrounded = false;
                }
            }
        }
        End:;

        float moveLeft = (transform.localScale.x < 0) ? -1 : 1;
        
        rb.velocity = new Vector2(enemyData.moveSpeed * moveLeft, rb.velocity.y);

        if (!isGrounded) {
            // If enemy is not grounded, flip its scale and position it in a way so that on the next frame, the enemy
            // will be back on the ground. (In this case, it iss 0.05 world units.)

            transform.localScale = new Vector3(moveLeft * -1, transform.localScale.y, transform.localScale.z);
            transform.position = new Vector2(transform.position.x + (0.05f *(moveLeft * -1)), transform.position.y);
        }
	}

    public void TakeDamage(float damage) {
        health -= damage;

        if (health <= 0) {
            Destroy(gameObject);
        }
    }
}
