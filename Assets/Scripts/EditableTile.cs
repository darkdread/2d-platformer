using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EditableTile : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnMouseDown() {
        //Destroy(gameObject);
    }

    private void OnMouseOver() {
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void OnMouseExit() {
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
