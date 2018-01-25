using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySkillData", menuName = "Lmao/Enemy Skill")]
public class EnemySkillObject : ScriptableObject {

    
    [TooltipAttribute("The minimum interval between each attack in seconds.")]
    [Range(0.1f, 100)]
    public float attackDelayMin;
    [TooltipAttribute("The maximum interval between each attack in seconds.")]
    [Range(0.1f, 100)]
    public float attackDelayMax;
    [TooltipAttribute("The damage of the skill")]
    public float damage;
    [TooltipAttribute("The id of the skill")]
    public int id;

}
