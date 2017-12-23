using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EditableTile : MonoBehaviour {

    public Tile tile;
    public SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
	
    private void OnMouseOver() {
        if (tile.IsEditable()) {
            spriteRenderer.color = tile.hoverColor;
        }
    }

    private void OnMouseExit() {
        if (tile.IsEditable()) {
            spriteRenderer.color = Color.white;
        }
    }
}
