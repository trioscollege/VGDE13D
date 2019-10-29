using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour 
{
	public float topSpeed = 150;
	public float maxReverseSpeed = -50;
	public float maxTurnAngle = 10;
	public float maxTorque = 10;
	public float decelerationTorque = 30;
	public Vector3 centerOfMassAdjustment = new Vector3(0f,-0.9f,0f);
	public float spoilerRatio = 0.1f;
	public WheelCollider wheelFL;
	public WheelCollider wheelFR;
	public WheelCollider wheelRL;
	public WheelCollider wheelRR;
	public Transform wheelTransformFL;
	public Transform wheelTransformFR;
	public Transform wheelTransformRL;
	public Transform wheelTransformRR;
	private Rigidbody body;

	void Start() {
		//lower center of mass for roll-over resistance
		body = GetComponent<Rigidbody>();
		body.centerOfMass += centerOfMassAdjustment;
	}
	
	// FixedUpdate is called once per physics frame
	void FixedUpdate() {
		//calculate max speed in KM/H (optimized calc)
		float currentSpeed = wheelRL.radius*wheelRL.rpm*Mathf.PI*0.12f;
		if(currentSpeed < topSpeed && currentSpeed > maxReverseSpeed) {
			//rear wheel drive.
			wheelRL.motorTorque = Input.GetAxis("Vertical") * maxTorque;
			wheelRR.motorTorque = Input.GetAxis("Vertical") * maxTorque;
		} else {
			//can't go faster, already at top speed that engine produces.
			wheelRL.motorTorque = 0;
			wheelRR.motorTorque = 0;
		}
		
		//Spoilers add down pressure based on the car’s speed. (Upside-down lift)
		Vector3 localVelocity = transform.InverseTransformDirection(body.velocity);
		body.AddForce(-transform.up * (localVelocity.z * spoilerRatio),ForceMode.Impulse);
		
		//front wheel steering
		wheelFL.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;
		wheelFR.steerAngle = Input.GetAxis("Horizontal")* maxTurnAngle;
		
		//apply deceleration when not pressing the gas or when breaking in either direction.
		if((Input.GetAxis("Vertical") <= -0.5f && localVelocity.z > 0)||(Input.GetAxis("Vertical") >= 0.5f && localVelocity.z < 0)) {
			wheelRL.brakeTorque = decelerationTorque + maxTorque;
			wheelRR.brakeTorque = decelerationTorque + maxTorque;
		} else if(Input.GetAxis("Vertical") == 0) {
			wheelRL.brakeTorque = decelerationTorque;
			wheelRR.brakeTorque = decelerationTorque;
		} else {
			wheelRL.brakeTorque = 0;
			wheelRR.brakeTorque = 0;
		}
	}
	
	void Update() {
		//rotate the wheels based on RPM
		float rotationThisFrame = 360*Time.deltaTime;
		wheelTransformFL.Rotate(wheelFL.rpm/rotationThisFrame,0,0);
		wheelTransformFR.Rotate(wheelFR.rpm/rotationThisFrame,0,0);
		wheelTransformRL.Rotate(wheelRL.rpm/rotationThisFrame,0,0);
		wheelTransformRR.Rotate(wheelRR.rpm/rotationThisFrame,0,0);
		
		//turn the wheels according to steering. But make sure you take into account the rotation being applied above.
		wheelTransformFL.localEulerAngles = new Vector3(wheelTransformFL.localEulerAngles.x, wheelFL.steerAngle - wheelTransformFL.localEulerAngles.z, wheelTransformFL.localEulerAngles.z);
		wheelTransformFR.localEulerAngles = new Vector3(wheelTransformFR.localEulerAngles.x, wheelFR.steerAngle - wheelTransformFR.localEulerAngles.z, wheelTransformFR.localEulerAngles.z);
	
		UpdateWheelPositions();
	}
	
	void UpdateWheelPositions() {
		//move wheels based on their suspension.
		WheelHit contact = new WheelHit();
		if(wheelFL.GetGroundHit(out contact)) {
			Vector3 temp = wheelFL.transform.position;
			temp.y = (contact.point + (wheelFL.transform.up*wheelFL.radius)).y;
			wheelTransformFL.position = temp;
		}
		if(wheelFR.GetGroundHit(out contact)) {
			Vector3 temp = wheelFR.transform.position;
			temp.y = (contact.point + (wheelFR.transform.up*wheelFR.radius)).y;
			wheelTransformFR.position = temp;
		}
		if(wheelRL.GetGroundHit(out contact)) {
			Vector3 temp = wheelRL.transform.position;
			temp.y = (contact.point + (wheelRL.transform.up*wheelRL.radius)).y;
			wheelTransformRL.position = temp;
		}
		if(wheelRR.GetGroundHit(out contact)) {
			Vector3 temp = wheelRR.transform.position;
			temp.y = (contact.point + (wheelRR.transform.up*wheelRR.radius)).y;
			wheelTransformRR.position = temp;
		}
	}
}
