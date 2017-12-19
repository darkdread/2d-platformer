using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameObjectInScene {
    public string name;
    public Vector3 scale;
    public Vector3 position;
    public Quaternion rotation;

    public GameObjectInScene(string name, Vector3 scale, Vector3 position, Quaternion rotation) {
        this.name = name;
        this.scale = scale;
        this.position = position;
        this.rotation = rotation;
    }
}