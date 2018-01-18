using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    private const float defaultJumpSpeed = 8f;
    private Game gameController;

    //the movement speed of the player
    public float moveSpeed;
    //the jumping speed of the player
    public float jumpSpeed;

    // Front of player
    public Transform playerFront;

    // Skills - Code logic credits: https://www.youtube.com/watch?v=8Xgao1qP7xw
    public float[] skillCooldown;
    public Image[] skillImage;

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

    private SpriteRenderer spriteRenderer;

    //to apply physics for the player
    public Rigidbody2D rb;

    public UnityEvent JumpEvent;

    // Use this for initialization
    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();
        gameController = FindObjectOfType<Game>();
        
        // Reset cooldown
        foreach(Image image in skillImage) {
            image.fillAmount = 1;
        }

        //player's initial health
        health = maxHealth;
        //player's initial respawn position
        respawnPosition = this.gameObject.transform.position;
    }

    // Update is called once per frame
    private void Update() {
        if (Main.EditorMode || Game.paused) return;

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
                }
            }
        }
        isGrounded = false;

        End:;

        //round the player's y axis
        if (isGrounded) {
            //print(Mathf.Round(transform.position.y * 100) / 100);
            //transform.position = new Vector3(transform.position.x, transform.position.y);
        }

        //if the player isn't getting knocked backed
        if (knockbackTimer <= 0) {

            if (spriteRenderer.color == Color.red) {
                spriteRenderer.color = Color.white;
            }

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
                Projectile projectile = LevelController.CreateProjectileTowardsDirection(gameController.ProjectileDictionary["kunai"], projectilePos, projectilePos + transform.localScale.x * Vector3.right * 2);
                LevelController.SetProjectileEnemyAgainst(projectile, "Enemy");
                projectile.transform.parent = Game.gameHolder;
            }

            for(int i = 0; i < skillImage.Length; i++) {
                if (skillImage[i].fillAmount < 1) {
                    skillImage[i].fillAmount += 1 / skillCooldown[i] * Time.deltaTime;
                }
            }

            // If the player uses skill 1
            if (Input.GetKeyDown(KeyCode.V) && skillImage[0].fillAmount >= 1) {
                skillImage[0].fillAmount = 0;

                Vector3 projectilePos = playerFront.position;
                Projectile projectile = LevelController.CreateProjectileTowardsDirection(gameController.ProjectileDictionary["shuriken"], projectilePos, projectilePos + transform.localScale.x * Vector3.right * 2);
                LevelController.SetProjectileEnemyAgainst(projectile, "Enemy");
                projectile.transform.parent = Game.gameHolder;
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

    // Allows other classes to knockback the player
    public void Knockback(Vector2 force) {

        // if the enemy is not already getting knocked
        if (knockbackTimer <= 0) {
            //set the length of the knockback
            knockbackTimer = knockbackLength;

            spriteRenderer.color = Color.red;
            rb.velocity = force;
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
    public void TakeDamage(float value) {
        //decrease player's health
        health -= value;

        LevelController.FlashScreen();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("DialogueSpeaker")) {
            DialogueSpeaker speaker = collision.GetComponent<DialogueSpeaker>();
            if (speaker.spoken) return;

            LevelController.ShowDialogue();
            DialogueScript.ChangeDialogueSpeaker(speaker.speakerName);
            DialogueScript.ChangeDialogueText(speaker.dialogueNumber, speaker.dialogueText);

            speaker.spoken = true;
        }
    }
}
