using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyDictionary : SerializedMonoBehaviour {

    [SerializeField]
    public Dictionary<string, GameObject> TileDictionary;
    public Dictionary<string, GameObject> ProjectileDictionary;

}
