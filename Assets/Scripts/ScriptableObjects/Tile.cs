﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Lmao/Tile")]
public class Tile : ScriptableObject {
    
    public Sprite sprite;
    [Tooltip("What is the prefab of the tile.")]
    public GameObject type;
    [Tooltip("What is the size of the tile in world units.")]
    public Vector2 size;
    [Tooltip("What color the sprite will be when hovered over.")]
    public Color hoverColor = Color.red;

    /*public Tile(GameObject _type) {
        type = _type;
        sprite = type.GetComponent<SpriteRenderer>().sprite;
    }*/

    public bool IsEditable() {
        if (!Editor.FilterTile(this.name)) {
            return true;
        }
        return false;
    }
	
}