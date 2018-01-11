using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public EnemyObject enemyData;
    public Transform groundCheckLeft, groundCheckRight;

    private Rigidbody2D rb;
    private Animator anim;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
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
        transform.localScale = new Vector3(moveLeft, transform.localScale.y, transform.localScale.z);

        if (!isGrounded) {
            print("test");
            transform.localScale = new Vector3(moveLeft * -1, transform.localScale.y, transform.localScale.z);
        }
	}
}
