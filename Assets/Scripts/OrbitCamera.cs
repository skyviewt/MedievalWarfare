﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class OrbitCamera : MonoBehaviour
{
	// Target to look at
	public Transform TargetLookAt;
 
	// Camera distance variables
	public float Distance = 15.0f;
	public float DistanceMin = 0.5f;
	public float DistanceMax = 15.0f;  
	float startingDistance = 0.0f;
	float desiredDistance = 0.0f;

	// Mouse variables
	float mouseX = 0.0f;
	float mouseY = 0.0f;
	public float X_MouseSensitivity = 5.0f;
	public float Y_MouseSensitivity = 5.0f;
	public float MouseWheelSensitivity = 5.0f;

	// Axis limit variables
	public float Y_MinLimit = 15.0f;
	public float Y_MaxLimit = 70.0f;   
	public float DistanceSmooth = 0.025f;
	float velocityDistance = 0.0f; 
	Vector3 desiredPosition = Vector3.zero;   
	public float X_Smooth = 0.05f;
	public float Y_Smooth = 0.1f;

	// Velocity variables
	float velX = 0.0f;
	float velY = 0.0f;
	float velZ = 0.0f;
	Vector3 position = Vector3.zero;
	

  // ######################################################################
  // MonoBehaviour Functions
  // ######################################################################

	void Start() 
	{
		//Distance = Mathf.Clamp(Distance, DistanceMin, DistanceMax);
		Distance = Vector3.Distance(TargetLookAt.transform.position, gameObject.transform.position);
		if (Distance > DistanceMax)
		  DistanceMax = Distance;
		startingDistance = DistanceMax;
		Reset();
	}

	void Update () {

		Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
		forward.y = 0;
		forward = forward.normalized;
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		
		Vector3 moveDirection = (h * right + v * forward).normalized;
		moveDirection *= 0.1f;
		TargetLookAt.transform.Translate(moveDirection);
	}

	// LateUpdate is called after all Update functions have been called.
	void LateUpdate()
	{
		if (TargetLookAt == null)
		   return;
 
		HandlePlayerInput();
 
		CalculateDesiredPosition();
 
		UpdatePosition();
	}
	

	void HandlePlayerInput()
	{
		// mousewheel deadZone
		float deadZone = 0.01f; 
 
		if (Input.GetMouseButton(0))
		{
		   mouseX += Input.GetAxis("Mouse X") * X_MouseSensitivity;
		   mouseY -= Input.GetAxis("Mouse Y") * Y_MouseSensitivity;
		}
	 
		// this is where the mouseY is limited - Helper script
		mouseY = ClampAngle(mouseY, Y_MinLimit, Y_MaxLimit);
 
		// get Mouse Wheel Input
		if (Input.GetAxis("Mouse ScrollWheel") < -deadZone || Input.GetAxis("Mouse ScrollWheel") > deadZone)
		{
		   desiredDistance = Mathf.Clamp(Distance - (Input.GetAxis("Mouse ScrollWheel") * MouseWheelSensitivity), 
													 DistanceMin, DistanceMax);
		}
	}
	
	void CalculateDesiredPosition()
	{
		// Evaluate distance
		Distance = Mathf.SmoothDamp(Distance, desiredDistance, ref velocityDistance, DistanceSmooth);
 
		// Calculate desired position -> Note : mouse inputs reversed to align to WorldSpace Axis
		desiredPosition = CalculatePosition(mouseY, mouseX, Distance);
	}

	Vector3 CalculatePosition(float rotationX, float rotationY, float distance)
	{
		Vector3 direction = new Vector3(0, 0, -distance);
		Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
		return TargetLookAt.position + (rotation * direction);
	}
	

	// update camera position
	void UpdatePosition()
	{
		float posX = Mathf.SmoothDamp(position.x, desiredPosition.x, ref velX, X_Smooth);
		float posY = Mathf.SmoothDamp(position.y, desiredPosition.y, ref velY, Y_Smooth);
		float posZ = Mathf.SmoothDamp(position.z, desiredPosition.z, ref velZ, X_Smooth);
		position = new Vector3(posX, posY, posZ);
 
		transform.position = position;
 
		transform.LookAt(TargetLookAt);
	}

	// Reset Mouse variables
	void Reset() 
	{
		mouseX = 0;
		mouseY = 0;
		Distance = startingDistance;
		desiredDistance = Distance;
	}

	// Clamps angle between a minimum float and maximum float value
	float ClampAngle(float angle, float min, float max)
	{
		while (angle < -360.0f || angle > 360.0f)
		{
		   if (angle < -360.0f)
			 angle += 360.0f;
		   if (angle > 360.0f)
			 angle -= 360.0f;
		}
 
		return Mathf.Clamp(angle, min, max);
	}
	
}

