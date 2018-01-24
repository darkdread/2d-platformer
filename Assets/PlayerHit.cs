using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : MonoBehaviour {

    private Player player;

    private void Awake() {
        player = GetComponentInParent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Enemy")) {
            IDamageableObject damageableObject = collision.GetComponentInParent<IDamageableObject>();

            float directionX = (player.transform.localScale.x > 0) ? 1 : -1;
            Vector2 knockbackForce = new Vector2(5 * directionX, Mathf.Abs(5));

            damageableObject.TakeDamage(player.damage);
            damageableObject.Knockback(knockbackForce);
        }
    }
}
