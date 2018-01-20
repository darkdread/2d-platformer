using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Lmao/Projectile")]
public class ProjectileObject : ScriptableObject {

    public float speed;
    public float knockbackForce;
    public float damage;

}
