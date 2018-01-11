using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour {
    private const float defaultJumpSpeed = 8f;
    private Game gameController;
    private LevelController levelController;

    //the movement speed of the player
    public float moveSpeed;
    //the jumping speed of the player
    public float jumpSpeed;

    // Front of player
    public Transform playerFront;

    //to check if the player is on the ground
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask[] realGround;
    public bool isGrounded;

    //the respawn position of the player
    public Vector3 respawnPosition;
    //the health & max health of the player
    public float health;
    public float maxHealth;

    //knockback force
    public float knockbackForce;
    //the amount of time to get knockbacked
    public float knockbackLength;
    //the amount of time to trigger knockback on player again
    private float knockbackTimer;

    //to animate the player
    private Animator myAnim;
    //to apply physics for the player
    public Rigidbody2D rb;

    public UnityEvent JumpEvent;

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();
        gameController = FindObjectOfType<Game>();
        levelController = FindObjectOfType<LevelController>();

        //player's initial health
        health = maxHealth;
        //player's initial respawn position
        respawnPosition = this.gameObject.transform.position;
    }

    // Update is called once per frame
    void Update() {
        if (Main.EditorMode) return;
        //get player's input on the horizontal axis.
        var x = Input.GetAxisRaw("Horizontal");

        //check if the player is on the ground.
        foreach (LayerMask mask in realGround) {
            Collider2D[] result = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, mask);
            
            // Make sure we aren't checking for the player itself
            foreach(Collider2D collider in result) {
                if (collider.gameObject.GetInstanceID() != this.gameObject.GetInstanceID()) {
                    isGrounded = true;
                    goto End;
                } else {
                    isGrounded = false;
                }
            }
        }
        End:;

        //round the player's y axis
        if (isGrounded) {
            //print(Mathf.Round(transform.position.y * 100) / 100);
            //transform.position = new Vector3(transform.position.x, transform.position.y);
        }

        //if the player isn't getting knocked backed
        if (knockbackTimer <= 0) {
            //if the player is moving left/right
            if (x != 0) {
                //if the player is moving to the right, set the movement speed to positive. Vice-versa.
                x = (x > 0) ? moveSpeed : -moveSpeed;
                //if the player is moving to the right, set the scale to positive. Vice-versa.
                float scaleX = (x > 0) ? 1 : -1;
                //scale the player accordingly.
                transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
                //move the player according to the movement speed.
                rb.velocity = new Vector2(x, rb.velocity.y);
            } else {
                //if the player isn't moving, set the velocity to 0.
                rb.velocity = new Vector2(0, rb.velocity.y);
            }

            //if the player presses the jump key and is on the ground
            if (Input.GetButtonDown("Jump") && isGrounded) {
                //the player jumps according to the jump speed.
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
                JumpEvent.Invoke();
            }

            // If the player throws a projectile
            if (Input.GetKeyDown(KeyCode.C)) {
                Vector3 projectilePos = playerFront.position;

                GameObject projectile = levelController.CreateProjectileTowardsDirection(gameController.ProjectileDictionary["shuriken"], projectilePos, projectilePos + transform.localScale.x * Vector3.right * 2);
                projectile.transform.parent = gameController.gameHolder.transform;
            }
        } else if (knockbackTimer > 0) {
            //if the player is getting knockedbacked, decrease the timer
            knockbackTimer -= Time.deltaTime;
        }

        //setting variables for animator so the animator knows what state to animate
        //myAnim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        //myAnim.SetBool("Ground", isGrounded);
    }

    //allows other classes to make the player jump
    public void Jump(float speed = defaultJumpSpeed) {
        rb.velocity = new Vector2(rb.velocity.x, speed);
    }

    //allows other classes to make the player get knockedbacked
    public void Knockback() {

        //if the player is not already getting knocked
        if (knockbackTimer <= 0) {
            //set the length of the knockback
            knockbackTimer = knockbackLength;

            //set the force, if the player is facing right, knock him to the left. Vice-versa
            float _knockbackForce = transform.localScale.x > 0 ? -knockbackForce : knockbackForce;
            rb.velocity = new Vector2(_knockbackForce, Mathf.Abs(_knockbackForce));
        }
    }

    public void DisableMovement(float seconds) {

        //if the player is not already getting knocked
        if (knockbackTimer <= 0) {
            //set the length of the knockback
            knockbackTimer = seconds;
        }
    }

    //allows other classes to damage the player
    public void DamagePlayer(float value) {
        //decrease player's health
        health -= value;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        //if the collided object is a kill plane
        /*if (collision.name == "KillPlane") {
            //damage the player (instantly kills her)
            this.DamagePlayer(this.health);
        }
        //if the collided object is a checkpoint (Flag)
        else if (collision.CompareTag("Checkpoint")) {
            //set the respawn position to the flag the player collided with
            respawnPosition = collision.transform.position;
        }*/

    }
}
