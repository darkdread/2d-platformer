using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : SerializedMonoBehaviour {
    
    //public List<SoundObject> soundObjects;
    private AudioSource source;
    public Dictionary<string, SoundObject[]> soundDict;

	// Use this for initialization
	void Start () {
        source = gameObject.AddComponent<AudioSource>();
	}

    public void PlaySound(SoundObject sound) {
        source.PlayOneShot(sound.clip, sound.volume);
    }

    public void PlaySoundAtRandom(string listName) {
        if (soundDict.ContainsKey(listName)) {
            SoundObject[] list = soundDict[listName];
            SoundObject soundObject = list[Random.Range(0, list.Length)];
            source.PlayOneShot(soundObject.clip, soundObject.volume);
        }
    }
}
