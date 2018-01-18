using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Credits - https://www.youtube.com/watch?v=QkisHNmcK7Y
public class Scroller : MonoBehaviour {

    public bool isParallax = true;
    public bool isScrolling = true;
    public bool disableLookAhead = true;
    public bool followPlayerY = false;
    public float backgroundSize;
    public float parallaxSpeed;

    private Transform cam;
    private Transform[] layers;
    private float viewZone = 10;
    private int leftIndex, rightIndex;
    private int posZ;
    private float lastCameraX;

    private static Transform player;
    private static float lastPlayerX;
    private static float followAhead;
    private static float smoothing;
    private static float xPosLimit;

    private void Awake() {
        cam = Camera.main.transform;
        lastCameraX = cam.position.x;
        posZ = (int) transform.position.z;
        layers = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++) {
            layers[i] = transform.GetChild(i);
        }

        leftIndex = 0;
        rightIndex = layers.Length - 1;
    }

    public static void SetPlayerTransform(Transform _player) {
        player = _player;
        lastPlayerX = player.position.x;
        CameraController cameraController = FindObjectOfType<CameraController>();
        followAhead = cameraController.followAhead;
        smoothing = cameraController.smoothing;
        xPosLimit = cameraController.xPosLimit;
    }
    

    private void Update() {
        if (isParallax) {
            float deltaX = cam.position.x - lastCameraX;

            if (!disableLookAhead)
                transform.position += Vector3.right * (deltaX * parallaxSpeed);

            if (disableLookAhead) {
                float facingRight = (player.transform.localScale.x > 0) ? followAhead : -followAhead;
                float xPos = player.position.x;

                // Player is facing left
                if (facingRight < 0) {

                    if (player.position.x - followAhead <= xPosLimit) {
                        if (player.position.x + followAhead <= xPosLimit) {
                            xPos = xPosLimit + followAhead;
                        }
                        xPos = xPosLimit + followAhead;
                    }

                } else {
                    // Camera starts moving near the edge of left side
                    if (player.position.x - followAhead <= xPosLimit) {
                        // Camera stops moving near the edge of left side
                        if (player.position.x + followAhead <= xPosLimit) {
                            xPos = xPosLimit + followAhead;
                        }
                        xPos = xPosLimit + followAhead;
                    }
                }

                xPos = player.position.x - followAhead <= xPosLimit ? xPosLimit + followAhead :
                    player.position.x + followAhead >= Game.gridWidth - xPosLimit ? Game.gridWidth - xPosLimit - followAhead:
                    player.position.x;

                deltaX = xPos;

                transform.position = Vector3.right * (deltaX * parallaxSpeed);
            }
        }

        
        //float yPos = player.transform.position.y >= yPosLimit ? player.transform.position.y : yPosLimit;

        lastCameraX = cam.position.x;

        if (isScrolling) {
            if (cam.position.x < layers[leftIndex].position.x + viewZone) {
                ScrollLeft();
            }

            if (cam.position.x > layers[rightIndex].position.x - viewZone) {
                ScrollRight();
            }
        }

        if (followPlayerY) {
            foreach (Transform cameraTransform in layers) {
                cameraTransform.position = new Vector3(cameraTransform.position.x, Camera.main.transform.position.y, cameraTransform.position.z);
            }
        }
    }

    private void ScrollLeft() {
        layers[rightIndex].position = new Vector3(layers[leftIndex].position.x - backgroundSize, 0, posZ);
        leftIndex = rightIndex;
        rightIndex--;
        if (rightIndex < 0) {
            rightIndex = layers.Length - 1;
        }
    }

    private void ScrollRight() {
        layers[leftIndex].position = new Vector3(layers[rightIndex].position.x + backgroundSize, 0, posZ);
        rightIndex = leftIndex;
        leftIndex++;
        if (leftIndex == layers.Length) {
            leftIndex = 0;
        }
    }
}
