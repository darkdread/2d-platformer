using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="ShooterData", menuName ="Lmao/Shooter")]
public class ShooterObject : ScriptableObject {

    public Projectile projectile;
    public float shotsPerSecond;

}
