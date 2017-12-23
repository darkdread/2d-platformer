using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Lmao/Sound")]
public class SoundObject : ScriptableObject {

    public AudioClip clip;
    [Range(0, 1)]
    public float volume = 1;

}
