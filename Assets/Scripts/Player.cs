using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IDamageableObject {
    void TakeDamage(float value);
    void Knockback(Vector2 force);
}


public class Player : MonoBehaviour, IDamageableObject {
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
    public GameObject[] skillImage;

    //to check if the player is on the ground
    public Transform groundCheckFront, groundCheckBack;
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

    // Ground movement effects
    public ParticleSystem particleSystem;
    private bool isParticleSystemPlaying;
    private bool justGrounded;

    const float skinWidth = .015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    private float horizontalRaySpacing;
    private float verticalRaySpacing;

    BoxCollider2D playerCollider;
    RaycastOrigins raycastOrigins;
    Bounds startingBounds;

    public UnityEvent JumpEvent;

    // Use this for initialization
    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();
        gameController = FindObjectOfType<Game>();
        playerCollider = GetComponent<BoxCollider2D>();

        skillImage[0] = GameObject.Find("Skill1Cooldown");
        startingBounds = playerCollider.bounds;

        // Reset cooldown
        foreach(GameObject obj in skillImage) {
            Image image = obj.GetComponent<Image>();
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

        UpdateRaycastOrigins();
        CalculateRaySpacing();

        //Vector2 horizontalCollision = new Vector2(transform.localScale.x / 5, 0);
        //HorizontalCollisions(horizontalCollision);

        Vector2 verticalCollision = new Vector2(0, -0.5f);

        //check if the player is on the ground.
        foreach (LayerMask mask in realGround) {
            Collider2D[] front = Physics2D.OverlapCircleAll(groundCheckFront.position, groundCheckRadius, mask);
            Collider2D[] back = Physics2D.OverlapCircleAll(groundCheckBack.position, groundCheckRadius, mask);
            Collider2D[] result = new Collider2D[front.Length + back.Length];

            front.CopyTo(result, 0);
            back.CopyTo(result, front.Length);

            // Make sure we aren't checking for the player itself
            foreach (Collider2D collider in result) {
                if (collider.gameObject.GetInstanceID() != this.gameObject.GetInstanceID()) {
                    if (VerticalCollisions(verticalCollision)) {
                        if (!isGrounded) justGrounded = true;
                        isGrounded = true;
                        UpdateMovementParticle(collider.gameObject);
                        goto End;
                    }
                }
            }
        }
        isGrounded = false;

        End:;

        // Stop particles from emitting if the player stopped running || started jumping
        if (isParticleSystemPlaying) {
            if (rb.velocity.x == 0 || !isGrounded) {
                particleSystem.Stop();
                isParticleSystemPlaying = false;
            }
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
            }

            // If the player reflects a projectile
            if (Input.GetKeyDown(KeyCode.F)) {
                Vector3 projectilePos = playerFront.position;
                Collider2D collider = Physics2D.OverlapCircle(projectilePos, 1f);

                if (collider) {
                    Projectile projectile = collider.gameObject.GetComponent<Projectile>();

                    if (projectile && projectile.IsEnemyOf("Player")) {
                        projectile.Reflect();
                    }
                }
            }

            // Player's skills
            for (int i = 0; i < skillImage.Length; i++) {
                Image image = skillImage[i].GetComponent<Image>();

                // If the player uses skill 1
                if (image.fillAmount >= 1) {
                    if (Input.GetKeyDown(KeyCode.V) && i == 0) {
                        image.fillAmount = 0;

                        Vector3 projectilePos = playerFront.position;
                        Projectile projectile = LevelController.CreateProjectileTowardsDirection(gameController.ProjectileDictionary["shuriken"], projectilePos, projectilePos + transform.localScale.x * Vector3.right * 2);
                        LevelController.SetProjectileEnemyAgainst(projectile, "Enemy");
                    }
                }
            }

            

        } else if (knockbackTimer > 0) {
            //if the player is getting knockedbacked, decrease the timer
            knockbackTimer -= Time.deltaTime;
        }

        for (int i = 0; i < skillImage.Length; i++) {
            Image image = skillImage[i].GetComponent<Image>();

            if (image.fillAmount < 1) {
                image.fillAmount += 1 / skillCooldown[i] * Time.deltaTime;
            }
        }

            //setting variables for animator so the animator knows what state to animate
            //myAnim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            //myAnim.SetBool("Ground", isGrounded);
    }

    private void LateUpdate() {

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount];
        int count = particleSystem.GetParticles(particles);
        for (int i = 0; i < count; i++) {

            // Get newly created particles and set their velocity, because the particle system is a bitch to deal with.
            if (particles[i].startLifetime - particles[i].remainingLifetime <= Time.deltaTime) {
                float yVel = (particles[i].remainingLifetime / particles[i].startLifetime) * 0.2f;
                float xVel = (particles[i].remainingLifetime / particles[i].startLifetime) * -rb.velocity.x / moveSpeed;
                
                particles[i].velocity = new Vector3(xVel, yVel, 0);
            }
        }

        particleSystem.SetParticles(particles, count);
    }


    // Since we're using polygoncollider, there's no way to detect what tile the player is on... 
    private void UpdateMovementParticle(GameObject ground) {
        string name = Regex.Replace(ground.name, @"\d", "");

        switch (name) {
            case "Grass":
                break;
        }

        if (!isParticleSystemPlaying) {
            if (Mathf.Abs(rb.velocity.x) > 0) {
                particleSystem.Play();
                isParticleSystemPlaying = true;
            } else if (justGrounded) {
                particleSystem.Emit(1);
                justGrounded = false;
            }
        }
    }

    //allows other classes to make the player jump
    public void Jump(float speed = defaultJumpSpeed) {
        rb.velocity = new Vector2(rb.velocity.x, speed);
    }

    // Allows other classes to knockback the player
    public void Knockback(Vector2 force) {

        // if the player is not already getting knocked
        if (knockbackTimer <= 0) {
            //set the length of the knockback
            knockbackTimer = knockbackLength;

            spriteRenderer.color = Color.red;
            rb.velocity = force;
        } else {
            //set the length of the knockback
            knockbackTimer = knockbackLength;

            // If the new knockback force is greater than the current knockback force, we replace the force with the greater one.

            float newX = Mathf.Abs(force.x) >= Mathf.Abs(rb.velocity.x) ? force.x : rb.velocity.x;
            float newY = Mathf.Abs(force.y) >= Mathf.Abs(rb.velocity.y) ? force.y : rb.velocity.y;

            rb.AddForce(force, ForceMode2D.Impulse);
            //rb.velocity = new Vector2(newX, newY);
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
        } else if (collision.CompareTag("Win")) {
            // Win Screen

            // After Win Screen back to menu?
            Game.WinGame();
        }
    }

    void HorizontalCollisions(Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, realGround[0]);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit) {
                velocity.x = (hit.distance - skinWidth) * directionX;
                print("collide horizontal");
                rayLength = hit.distance;
            }
        }
    }

    bool VerticalCollisions(Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            //rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            rayOrigin += Vector2.right * (i * (raycastOrigins.bottomRight.x - raycastOrigins.bottomLeft.x) / (verticalRayCount - 1));
            rayOrigin += Vector2.up * (i * (raycastOrigins.bottomRight.y - raycastOrigins.bottomLeft.y) / (verticalRayCount - 1));

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, realGround[0]);

            print(i * (raycastOrigins.bottomRight.y - raycastOrigins.bottomLeft.y)/ (verticalRayCount - 1));

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit) {
                //velocity.y = (hit.distance - skinWidth) * directionY;

                //rayLength = hit.distance;

                float playerMove = Input.GetAxisRaw("Horizontal");
                
                // If player is facing right

                if (transform.localScale.x > 0) {
                    // Left-most ray

                    if (i == 0) {
                        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                        // If going up
                        float rotateDegree = (rb.velocity.y > 0) ? slopeAngle : -slopeAngle;
                        transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotateDegree));

                        return true;
                    }
                } else if (transform.localScale.x < 0) {
                    // Right-most ray

                    if (i == verticalRayCount - 1) {
                        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                        float rotateDegree = (rb.velocity.y < 0) ? slopeAngle : -slopeAngle;
                        transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotateDegree));

                        return true;
                    }
                } else {

                    if (i == 2) {
                        float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                        // If going up

                        float rotateDegree = (rb.velocity.y > 0) ? slopeAngle : -slopeAngle;
                        transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotateDegree));

                        return true;
                    }
                }

            }
        }
        return false;
    }

    void UpdateRaycastOrigins() {
        Bounds bounds = playerCollider.bounds;
        bounds.Expand(skinWidth * -2);

        Vector2 boxPos = playerCollider.bounds.center;
        float width = playerCollider.size.x;
        float height = playerCollider.size.y;
        float rotatedAngle = transform.rotation.eulerAngles.z;

        //print(boxPos.x - width/2 * Mathf.Cos(rotatedAngle * Mathf.Deg2Rad) + height/2 * Mathf.Sin(rotatedAngle * Mathf.Deg2Rad));
        //print(boxPos.y - width / 2 * Mathf.Sin(rotatedAngle * Mathf.Deg2Rad) - height / 2 * Mathf.Cos(rotatedAngle * Mathf.Deg2Rad));

        raycastOrigins.bottomLeft = new Vector2(boxPos.x - width / 2 * Mathf.Cos(rotatedAngle * Mathf.Deg2Rad) + height / 2 * Mathf.Sin(rotatedAngle * Mathf.Deg2Rad), boxPos.y - width / 2 * Mathf.Sin(rotatedAngle * Mathf.Deg2Rad) - height / 2 * Mathf.Cos(rotatedAngle * Mathf.Deg2Rad));

        raycastOrigins.bottomRight = new Vector2(boxPos.x + width / 2 * Mathf.Cos(rotatedAngle * Mathf.Deg2Rad) + height / 2 * Mathf.Sin(rotatedAngle * Mathf.Deg2Rad), boxPos.y + width / 2 * Mathf.Sin(rotatedAngle * Mathf.Deg2Rad) - height / 2 * Mathf.Cos(rotatedAngle * Mathf.Deg2Rad));

        raycastOrigins.topLeft = new Vector2(raycastOrigins.bottomLeft.x, raycastOrigins.bottomLeft.y + playerCollider.size.y);

        raycastOrigins.topRight = new Vector2(raycastOrigins.bottomLeft.x + playerCollider.size.x, raycastOrigins.bottomLeft.y + playerCollider.size.y);

        //print(raycastOrigins.bottomLeft);
        //print(raycastOrigins.bottomRight);

        /*
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        */
    }

    void CalculateRaySpacing() {
        Bounds bounds = playerCollider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = startingBounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = startingBounds.size.x / (verticalRayCount - 1);

        //print(verticalRaySpacing);
    }

    struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
