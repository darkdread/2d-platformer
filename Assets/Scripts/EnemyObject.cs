﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Lmao/Enemy")]
public class EnemyObject : ScriptableObject {

    [TooltipAttribute("Health of enemy.")]
    public float health;
    [TooltipAttribute("Movement speed of enemy.")]
    public float moveSpeed;
    [TooltipAttribute("The abilities that the enemy has.")]
    public List<EnemySkillObject> skills;
    [TooltipAttribute("The minimum interval between each skill in seconds.")]
    [Range(0.1f, 100)]
    public float skillDelayMin;
    [TooltipAttribute("The maximum interval between each skill in seconds.")]
    [Range(0.1f, 100)]
    public float skillDelayMax;
    [TooltipAttribute("The radius to check if enemy is on the ground.")]
    public float groundCheckRadius;
    [TooltipAttribute("The layer(s) which are counted as being on ground.")]
    public LayerMask realGround;

}
