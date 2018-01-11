using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {

    private List<GameObject> projectileList = new List<GameObject>();
    
	public GameObject CreateProjectileTowardsDirection(GameObject type, Vector3 position, Vector3 targetPosition) {
        GameObject projectile = Instantiate<GameObject>(type, position, Quaternion.identity);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        float force = projectile.GetComponent<Projectile>().projectileData.force;
        Vector3 heading = targetPosition - position;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance;

        projectileRb.AddForce(direction * force, ForceMode2D.Impulse);
        projectileList.Add(projectile);

        return projectile;
    }
}
