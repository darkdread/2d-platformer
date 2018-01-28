using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit : MonoBehaviour {

    private IDamageableObject damageableObject;
    private Transform obj;

    private void Awake() {
        damageableObject = GetComponentInParent<IDamageableObject>();
        obj = transform.parent.parent;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        print(collision.tag);
        if (collision.CompareTag("Enemy")) {
            IDamageableObject other = collision.GetComponentInParent<IDamageableObject>();

            float directionX = (obj.localScale.x > 0) ? 1 : -1;
            Vector2 knockbackForce = new Vector2(5 * directionX, Mathf.Abs(5));

            other.TakeDamage(10);
            other.Knockback(knockbackForce);
        }
    }
}
