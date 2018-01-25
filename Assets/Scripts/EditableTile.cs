using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EditableTile : MonoBehaviour {

    public Tile tile;
    public SpriteRenderer spriteRenderer;
    public Color defaultColor;

    // Use this for initialization
    void Awake () {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColor = spriteRenderer.color;
    }
	
    private void OnMouseOver() {
        if (tile.IsEditable()) {
            spriteRenderer.color = tile.hoverColor;
        }
    }

    private void OnMouseExit() {
        if (tile.IsEditable()) {
            spriteRenderer.color = defaultColor;
        }
    }
}
