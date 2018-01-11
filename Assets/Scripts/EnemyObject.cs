using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Lmao/Enemy")]
public class EnemyObject : ScriptableObject {

    
    public float moveSpeed;
    public float groundCheckRadius;
    public LayerMask[] realGround;

}
