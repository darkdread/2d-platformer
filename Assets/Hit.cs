using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit : MonoBehaviour {

    private IDamageableObject self;
    private Transform obj;
    private Animator animator;
    private Collider2D thisCollider;

    private void Awake() {
        self = GetComponentInParent<IDamageableObject>();
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

            if (blockCollider != null) {
                bool isColliderInBlockCollider = Physics2D.IsTouching(thisCollider, blockCollider);

                if (isColliderInBlockCollider) {
                    self.Knockback(knockbackForce * -1);
                    other.Knockback(knockbackForce / 4);
                    return;
                }
            }

            if (other.BloodSpray != null) {
                GameObject blood = Instantiate<GameObject>(other.BloodSpray, collision.transform.position, Quaternion.identity, Game.gameHolder);
                blood.transform.position = blood.transform.position + new Vector3(directionX, 0, 0);
                ParticleSystem ps = blood.GetComponent<ParticleSystem>();
                ParticleSystem.VelocityOverLifetimeModule vel = ps.velocityOverLifetime;
                ParticleSystem.MinMaxCurve curve = new ParticleSystem.MinMaxCurve(vel.x.constantMin * directionX, vel.x.constantMax * directionX);
                vel.x = curve;

                Destroy(blood, 5);
            }
            other.TakeDamage(10);
            other.Knockback(knockbackForce);
        }
    }
}
