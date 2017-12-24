using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingPlatform : MonoBehaviour {
    
    //the default sprite
    public Sprite defaultState;
    //the sprite when it is pressed down
    public Sprite fire;
    //the ground detector
    public Transform groundDetector;
    //the layer to detect
    public LayerMask layerMask;
    //the detection radius
    public float groundRadius;

    //the collider when it's ready (bigger)
    public BoxCollider2D defaultCollider;
    //the collider when it's firing (smaller)
    public BoxCollider2D fireCollider;
    //this collider
    private BoxCollider2D[] boxCollider2D;
    private Vector2[] colliders;

    //how much the impact will be
    public float force;

    //amount of time it takes to activate the platform
    public float activationTimer;
    //amount of time it takes to activate the platform
    private float activateTimer;
    //the sprite renderer for this game object
    private SpriteRenderer spriteRenderer;

    // The collision results returned when searching for colliders during Physics2D.OverlapCircle
    private Collider2D[] collisionResults;
    private bool isInArray;

    // Use this for initialization
    void Start () {
        //default timer
        activateTimer = activationTimer;
        //renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        //collider -> Both the trigger and the default
        boxCollider2D = GetComponents<BoxCollider2D>();

        // Clone the colliders (They should not be references, or else if I change the collider, both of them will change)
        // Lazy coding OMEGALUL
        colliders = new Vector2[4];
        colliders[0] = fireCollider.size;
        colliders[1] = fireCollider.offset;
        colliders[2] = fireCollider.size;
        colliders[3] = fireCollider.offset;

    }
	
	// Update is called once per frame
	void Update () {
        if (Main.EditorMode) return;

        // Get an array of the objects above the platform
        Collider2D[] smthAbove = Physics2D.OverlapCircleAll(groundDetector.position, groundRadius, layerMask);

        if (smthAbove.Length > 0) {
            //decrease the timer
            activateTimer -= Time.deltaTime;

            if (spriteRenderer.sprite != fire) {
                //swap to firing state
                spriteRenderer.sprite = fire;
                //change the collision box size to fit the firing state
                foreach (BoxCollider2D collider in boxCollider2D) {
                    collider.size = colliders[2];
                    collider.offset = colliders[3];
                }
            }

            //fire!
            if (activateTimer <= 0) {

                //apply force to rigidbody
                foreach (Collider2D obj in smthAbove) {
                    obj.GetComponent<Rigidbody2D>().AddForce(transform.up * force, ForceMode2D.Impulse);
                    if (obj.GetComponent<Player>()) {
                        obj.GetComponent<Player>().DisableMovement(0.3f);
                    }
                }

                ResetPlatform();
            }
        } else {
            ResetPlatform();
        }
    }

    private void ResetPlatform() {
        if (spriteRenderer.sprite == defaultState)
            return;

        //reset the timer
        activateTimer = activationTimer;
        
        //reset the sprite
        spriteRenderer.sprite = defaultState;

        //reset the collision box
        foreach (BoxCollider2D collider in boxCollider2D) {
            collider.size = colliders[0];
            collider.offset = colliders[1];
        }

    }
}
