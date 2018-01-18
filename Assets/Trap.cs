using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour {

    public DamageObject damageData;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            IDamageableObject damageableObject = collision.GetComponent<IDamageableObject>();

            if (damageableObject != null) {
                damageableObject.TakeDamage(damageData.damage);
                float directionX = (collision.transform.localScale.x > 0) ? -1 : 1;
                Vector2 knockbackForce = new Vector2(damageData.knockbackForce * directionX, Mathf.Abs(damageData.knockbackForce));
                damageableObject.Knockback(knockbackForce);
            }
        }
    }
}
