using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit : MonoBehaviour {

    private IDamageableObject damageableObject;
    private Transform obj;
    private Animator animator;
    private Collider2D thisCollider;

    private void Awake() {
        damageableObject = GetComponentInParent<IDamageableObject>();
        obj = transform.parent.parent;
        animator = obj.GetComponent<Animator>();
        thisCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        IDamageableObject other = collision.GetComponent<IDamageableObject>();
        bool isKnockedBacked = animator.GetBool("Knockedbacked");

        if (other != null && !isKnockedBacked) {

            // Setting up the knockback direction & force
            float directionX = (obj.localScale.x > 0) ? 1 : -1;
            Vector2 knockbackForce = new Vector2(5 * directionX, Mathf.Abs(5));

            // Find if there is a block collider on the collided object.
            // For example: If the enemy hits the player, we check if the player has a block collider.
            // If it exists, we check if the enemy's hit collider is touching the player's block collider.
            BoxCollider2D blockCollider = collision.transform.Find("Colliders") ? collision.transform.Find("Colliders").Find("Block").GetComponent<BoxCollider2D>() : null;
            
            bool isColliderInBlockCollider = Physics2D.IsTouching(thisCollider, blockCollider);

            
            if (isColliderInBlockCollider) {
                damageableObject.Knockback(knockbackForce*-1);
                other.Knockback(knockbackForce/4);
                return;
            }

            other.TakeDamage(10);
            other.Knockback(knockbackForce);
        }
    }
}
