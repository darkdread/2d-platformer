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
    }
    

    private void Update() {
        if (isParallax) {
            float deltaX = cam.position.x - lastCameraX;

            if (!disableLookAhead)
                transform.position += Vector3.right * (deltaX * parallaxSpeed);

            if (disableLookAhead) {
                deltaX = player.position.x;

                transform.position = Vector3.right * (deltaX * parallaxSpeed);
            }
        }

        lastCameraX = cam.position.x;
        lastPlayerX = player.position.x;

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
        int lastRight = rightIndex;
        //layers[rightIndex].position = Vector3.right * (layers[leftIndex].position.x - backgroundSize);
        layers[rightIndex].position = new Vector3(layers[leftIndex].position.x - backgroundSize, 0, posZ);
        leftIndex = rightIndex;
        rightIndex--;
        if (rightIndex < 0) {
            rightIndex = layers.Length - 1;
        }
    }

    private void ScrollRight() {
        int lastLeft = leftIndex;
        //layers[leftIndex].position = Vector3.right * (layers[rightIndex].position.x + backgroundSize);
        layers[leftIndex].position = new Vector3(layers[rightIndex].position.x + backgroundSize, 0, posZ);
        rightIndex = leftIndex;
        leftIndex++;
        if (leftIndex == layers.Length) {
            leftIndex = 0;
        }
    }
}
