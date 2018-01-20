using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour {

    public DamageObject damageData;

    private void OnTriggerStay2D(Collider2D collision) {
        if (Main.EditorMode) return;

        if (collision.CompareTag("Player")) {
            IDamageableObject damageableObject = collision.GetComponent<IDamageableObject>();

            if (damageableObject != null) {
                damageableObject.TakeDamage(damageData.damage);
                float directionX = (collision.transform.localScale.x > 0) ? -1 : 1;
                float directionY = (int) transform.up.y == 0 ? 1 : transform.up.y;
                //print(directionY);

                Vector2 knockbackForce = new Vector2(damageData.knockbackForce * directionX, damageData.knockbackForce * directionY);
                damageableObject.Knockback(knockbackForce);
            }
        }
    }
}
