using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EditableTile : MonoBehaviour {

    Tile tile;
    SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start () {
        tile = new Tile(gameObject);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnMouseDown() {
        //Destroy(gameObject);
    }

    private void OnMouseOver() {
        
        if (tile.IsEditable()) {
            spriteRenderer.color = Color.red;
        }
    }

    private void OnMouseExit() {
        if (tile.IsEditable()) {
            spriteRenderer.color = Color.white;
        }
    }
}
