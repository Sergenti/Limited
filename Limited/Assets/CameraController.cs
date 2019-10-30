﻿using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera cameraObject;
    public float maxPanSpeed = 5f;
    public float accelerationRate = 0.03f;
    private Vector2 currentPanSpeed = Vector2.zero;
    public int panBorderThickness = 20;
    public Vector2 panLimit;

    public float scrollSpeed = 2f;
    public float scrollLimitMin = 2f;
    public float scrollLimitMax = 5f;

    // Update is called once per frame
    void Update()
    {
        // Using Vector3 to avoid camera being collapsed on the tilemap, making it see the background
        Vector3 position = transform.position;
        bool isMoving = false;

        // Get input and apply movement
        if(Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness){
            currentPanSpeed.y += accelerationRate;
            isMoving = true;
        }
        if(Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness){
            currentPanSpeed.y -= accelerationRate;
            isMoving = true;
        }
        if(Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness){
            currentPanSpeed.x -= accelerationRate;
            isMoving = true;
        }
        if(Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness){
            currentPanSpeed.x += accelerationRate;
            isMoving = true;
        }

        if(currentPanSpeed.magnitude > maxPanSpeed){
            Vector2.ClampMagnitude(currentPanSpeed, maxPanSpeed);
        }
        position.x += currentPanSpeed.x * Time.deltaTime;
        position.y += currentPanSpeed.y * Time.deltaTime;

        if(currentPanSpeed.x > 0 && !isMoving) {
            currentPanSpeed.x -= accelerationRate;
            // prevent oscillation
            if(currentPanSpeed.x < 0){
                currentPanSpeed.x = 0;
            }
        }
        if(currentPanSpeed.x < 0 && !isMoving) {
            currentPanSpeed.x += accelerationRate;
            if(currentPanSpeed.x > 0){
                currentPanSpeed.x = 0;
            }
        }
        if(currentPanSpeed.y > 0 && !isMoving) {
            currentPanSpeed.y -= accelerationRate;
            if(currentPanSpeed.y < 0){
                currentPanSpeed.y = 0;
            }
        }
        if(currentPanSpeed.y < 0 && !isMoving) {
            currentPanSpeed.y += accelerationRate;
            if(currentPanSpeed.y > 0){
                currentPanSpeed.y = 0;
            }
        }

        // Get scroll input to zoom in and out
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cameraObject.orthographicSize -= scroll * scrollSpeed * 100f * Time.deltaTime;

        // reset speed component if camera is at the limit 
        if(position.x > panLimit.x || position.x < -panLimit.x){
            currentPanSpeed.x = 0;
        }
        if(position.y > panLimit.y || position.y < -panLimit.y){
            currentPanSpeed.y = 0;
        }
        
        // Limit camera movement
        position.x = Mathf.Clamp(position.x, -panLimit.x, panLimit.x);
        position.y = Mathf.Clamp(position.y, -panLimit.y, panLimit.y);
        cameraObject.orthographicSize = Mathf.Clamp(cameraObject.orthographicSize, scrollLimitMin, scrollLimitMax);


        transform.position = position;
    }
}