using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Lmao/Enemy")]
public class EnemyObject : ScriptableObject {

    [TooltipAttribute("Health of enemy.")]
    public float health;
    [TooltipAttribute("Movement speed of enemy.")]
    public float moveSpeed;
    [TooltipAttribute("The interval between each attack in seconds.")]
    public float attackDelay;
    [TooltipAttribute("The radius to check if enemy is on the ground.")]
    public float groundCheckRadius;
    [TooltipAttribute("The layer(s) which are counted as being on ground.")]
    public LayerMask realGround;

}
