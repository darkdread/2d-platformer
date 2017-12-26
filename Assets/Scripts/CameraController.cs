
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    //the variable which holds the player
    public GameObject player;
    //the world camera
    public Camera worldCamera;
    //the amount of units to move ahead
    public float followAhead;
    //how long it will take for the camera to transition
    public float smoothing;
    public Material mat;

    //the position to move the camera
    private Vector3 targetPosition;

    public float moveSpeed = 5f;
    public float zoomSpeed = 1;
    public float targetOrtho;
    public float smoothSpeed = 2.0f;
    public float minOrtho = 1.0f;
    public float maxOrtho = 16.875f;

    // Use this for initialization
    void Start () {
        targetOrtho = worldCamera.orthographicSize;
    }
	
	// Update is called once per frame
	void Update () {

        if (Main.EditorMode) {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f) {
                targetOrtho -= scroll * zoomSpeed;
                targetOrtho = Mathf.Clamp(targetOrtho, minOrtho, maxOrtho);
            }
        }

        worldCamera.orthographicSize = Mathf.MoveTowards(worldCamera.orthographicSize, targetOrtho, smoothSpeed * Time.deltaTime);

        if (Main.EditorMode && Editor.allowCameraMovement) {
            float xCamera = Input.GetAxisRaw("Horizontal") * moveSpeed;
            float yCamera = Input.GetAxisRaw("Vertical") * moveSpeed;
            targetPosition = new Vector3(transform.position.x + xCamera, transform.position.y + yCamera, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }

        if (player == null) return;
        //if the player is facing right, move the camera to the right. Vice-versa
        float facingRight = (player.transform.localScale.x > 0) ? followAhead : -followAhead;
        //if the player is on the ground, set the camera y position to 0. Or else, follow the player
        float yPos = player.transform.position.y;// > 0.1? player.transform.position.y: 0;

        //the position to move the camera
        targetPosition = new Vector3(player.transform.position.x + facingRight, yPos, transform.position.z);

        //move the camera with some smoothing
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
	}
}
