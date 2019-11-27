﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

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

	// screenshake
	private float shakeDuration = 0f;
	private float shakeMagnitude = 0.1f;
	private float dampingSpeed = 1f;
	private Vector3 initialPosition;

	// click movement
	private float moveDuration = 0.3f;
	private Vector3 moveInitial;
	private Vector3 moveDestination;

	// Update is called once per frame
	void Update()
	{
		// Using Vector3 to avoid camera being collapsed on the tilemap, making it see the background
		Vector3 position = transform.position;
		bool isMoving = false;

		// Get input and apply movement
		if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness)
		{
			currentPanSpeed.y += accelerationRate;
			isMoving = true;
		}
		if (Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness)
		{
			currentPanSpeed.y -= accelerationRate;
			isMoving = true;
		}
		if (Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness)
		{
			currentPanSpeed.x -= accelerationRate;
			isMoving = true;
		}
		if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness)
		{
			currentPanSpeed.x += accelerationRate;
			isMoving = true;
		}

		if (currentPanSpeed.magnitude > maxPanSpeed)
		{
			Vector2.ClampMagnitude(currentPanSpeed, maxPanSpeed);
		}
		position.x += currentPanSpeed.x * Time.deltaTime;
		position.y += currentPanSpeed.y * Time.deltaTime;

		if (currentPanSpeed.x > 0 && !isMoving)
		{
			currentPanSpeed.x -= accelerationRate;
			// prevent oscillation
			if (currentPanSpeed.x < 0)
			{
				currentPanSpeed.x = 0;
			}
		}
		if (currentPanSpeed.x < 0 && !isMoving)
		{
			currentPanSpeed.x += accelerationRate;
			if (currentPanSpeed.x > 0)
			{
				currentPanSpeed.x = 0;
			}
		}
		if (currentPanSpeed.y > 0 && !isMoving)
		{
			currentPanSpeed.y -= accelerationRate;
			if (currentPanSpeed.y < 0)
			{
				currentPanSpeed.y = 0;
			}
		}
		if (currentPanSpeed.y < 0 && !isMoving)
		{
			currentPanSpeed.y += accelerationRate;
			if (currentPanSpeed.y > 0)
			{
				currentPanSpeed.y = 0;
			}
		}

		// Get scroll input to zoom in and out
		float scroll = Input.GetAxis("Mouse ScrollWheel");

		if (scroll != 0f)
		{
			// prevents scrolling through UI (mostly for the scroll menu in BuildDialogBox)
			if (EventSystem.current.IsPointerOverGameObject())
			{
				//Debug.Log("Scrolled on UI.");
			}
			else
			{
				cameraObject.orthographicSize -= scroll * scrollSpeed * 64f * Time.deltaTime;
			}

		}

		// reset speed component if camera is at the limit 
		if (position.x > panLimit.x || position.x < -panLimit.x)
		{
			currentPanSpeed.x = 0;
		}
		if (position.y > panLimit.y || position.y < -panLimit.y)
		{
			currentPanSpeed.y = 0;
		}

		// Limit camera movement
		position.x = Mathf.Clamp(position.x, -panLimit.x, panLimit.x);
		position.y = Mathf.Clamp(position.y, -panLimit.y, panLimit.y);
		cameraObject.orthographicSize = Mathf.Clamp(cameraObject.orthographicSize, scrollLimitMin, scrollLimitMax);

		// screen shake
		if (shakeDuration > 0)
		{
			var random = Random.insideUnitCircle;
			transform.position = initialPosition + new Vector3(random.x, random.y, 0) * shakeMagnitude;

			shakeDuration -= Time.deltaTime * dampingSpeed;
		}
		else
		{
			shakeDuration = 0f;
			transform.position = position;
		}
	}

	public void TriggerShake(float duration = 0.1f, float magnitude = 0.1f)
	{
		shakeDuration = duration;
		shakeMagnitude = magnitude;
		initialPosition = transform.position;
	}

	public void MoveTo(Vector3 destination)
	{
		// don't change the z position of the camera, only move on x and y axis
		moveDestination = new Vector3(destination.x, destination.y, transform.position.z);
		moveInitial = transform.position;

		// calculate duration in function of the distance to cover, we also apply a maximum duration
		float distanceToCover = GameSystem.EuclideanDistance(moveInitial, moveDestination);
		float divider = 10f;
		float maxDuration = 0.4f;
		moveDuration = distanceToCover > maxDuration / divider ? maxDuration : distanceToCover / divider;

		StartCoroutine("MoveToRoutine");

	}

	private IEnumerator MoveToRoutine()
	{
		for (float t = 0.01f; t < moveDuration; t += Time.deltaTime)
		{
			// fade out the alpha value
			transform.position = Vector3.Lerp(moveInitial, moveDestination, t / moveDuration);
			yield return null;
		}
	}
}