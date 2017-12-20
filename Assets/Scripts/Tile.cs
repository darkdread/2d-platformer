using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {

    public Sprite sprite;
    public GameObject type;
    public int x, y;

    public Tile(GameObject _type) {
        type = _type;
        sprite = type.GetComponent<SpriteRenderer>().sprite;
    }

    public bool IsEditable() {
        if (!Editor.FilterTile(type.name)) {
            return true;
        }
        return false;
    }
	
}
