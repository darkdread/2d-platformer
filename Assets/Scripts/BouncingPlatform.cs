using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingPlatform : MonoBehaviour {
    
    //the default sprite
    public Sprite ready;
    //the sprite when it is pressed down
    public Sprite fire;
    //the ground detector
    public Transform groundDetector;
    //the layer to detect
    public LayerMask layerMask;
    //the detection radius
    public float groundRadius;

    //the collider when it's ready (bigger)
    public BoxCollider2D readyCollider;
    //the collider when it's firing (smaller)
    public BoxCollider2D fireCollider;
    //this collider
    private BoxCollider2D[] boxCollider2D;

    //how much the impact will be
    public float force;

    //amount of time it takes to activate the platform
    public float activationTimer;
    //amount of time it takes to activate the platform
    private float activateTimer;
    //the sprite renderer for this game object
    private SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start () {
        //default timer
        activateTimer = activationTimer;
        //renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        //collider -> Both the trigger and the default
        boxCollider2D = GetComponents<BoxCollider2D>();
    }
	
	// Update is called once per frame
	void Update () {

    }

    private void OnTriggerStay2D(Collider2D collision) {
        //if the collider is itself, don't do anything
        if (collision.CompareTag("BouncingPlatform")) return;

        if (Mathf.Abs(collision.transform.position.y - transform.position.y) > 0.5) {
            //decrease the timer
            activateTimer -= Time.deltaTime;

            if (spriteRenderer.sprite != fire) {
                //swap to firing state
                spriteRenderer.sprite = fire;
                //change the collision box size to fit the firing state
                foreach (BoxCollider2D collider in boxCollider2D) {
                    collider.size = fireCollider.size;
                }
            }

            //fire!
            if (activateTimer <= 0) {
                //force value
                Vector2 force = new Vector2(0, this.force);
                //apply force to rigidbody
                collision.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);

                //reset the platform
                OnTriggerExit2D(collision.GetComponent<Collider2D>());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        //if the collider is itself, don't do anything
        if (collision.CompareTag("BouncingPlatform") || spriteRenderer.sprite == ready) return;

        bool smthAbove = Physics2D.OverlapCircle(groundDetector.position, groundRadius, layerMask);

        //if the collided target is above this platform or the timer have started and something left
        if (smthAbove || activateTimer > 0) {
            //reset the timer
            activateTimer = activationTimer;

            if (spriteRenderer.sprite != ready) {
                //reset the sprite
                spriteRenderer.sprite = ready;

                //reset the collision box
                foreach (BoxCollider2D collider in boxCollider2D) {
                    collider.size = readyCollider.size;
                }
            }
        }
    }
}
