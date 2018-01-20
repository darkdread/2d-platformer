using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour {

    public ShooterObject shooterData;
    public float nextFire;
    
	private void Update () {
        if (Game.paused || Main.EditorMode) return;

        nextFire -= Time.deltaTime;

        if (nextFire <= 0) {

            //print(string.Format("transform.right: {0}, transform.up: {1}", transform.right, transform.up));
            Vector3 dir = transform.up;
            //print(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            Projectile projectile = LevelController.CreateProjectileTowardsAngle(Game.current.ProjectileDictionary["kunai"], transform.position + dir, transform.position + dir * 2f, transform.rotation.eulerAngles.z);
            projectile.AddEnemyTag("Player");

            //print(transform.position + dir * 2f);

            nextFire = 1 / shooterData.shotsPerSecond;
        }
	}
}
