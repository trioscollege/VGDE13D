using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour 
{
	public int numberOfGears;
	public float topSpeed = 150;
	public float maxReverseSpeed = -50;
	public float maxBrakeTorque = 100;
	public float maxTurnAngle = 10;
	public float maxTorque = 10;
	public float decelerationTorque = 30;
	public Vector3 centerOfMassAdjustment = new Vector3(0f,-0.9f,0f);
	public float spoilerRatio = 0.1f;
	public float handbrakeForwardSlip = 0.04f;
	public float handbrakeSidewaysSlip = 0.08f;
	public WheelCollider wheelFL;
	public WheelCollider wheelFR;
	public WheelCollider wheelBL;
	public WheelCollider wheelBR;
	public Transform wheelTransformFL;
	public Transform wheelTransformFR;
	public Transform wheelTransformBL;
	public Transform wheelTransformBR;
	public GameObject leftBrakeLight;
	public GameObject rightBrakeLight;
	public Texture2D idleLightTex;
	public Texture2D brakeLightTex;
	public Texture2D reverseLightTex;
	public Texture2D speedometer;
	public Texture2D needle;
	private bool applyHandbrake=false;
	private float currentSpeed;
	private float gearSpread;
	
	void Start() {
		//calculate the spread of top speed over the number of gears.
		gearSpread = topSpeed / numberOfGears;
		
		//lower center of mass for roll-over resistance
		GetComponent<Rigidbody>().centerOfMass += centerOfMassAdjustment;
	}
	
	void SetSlipValues(float forward, float sideways) {
		WheelFrictionCurve tempStruct = wheelBR.forwardFriction;
		tempStruct.stiffness = forward;
		wheelBR.forwardFriction = tempStruct;
		tempStruct = wheelBR.sidewaysFriction;
		tempStruct.stiffness = sideways;
		wheelBR.sidewaysFriction = tempStruct;
		
		tempStruct = wheelBL.forwardFriction;
		tempStruct.stiffness = forward;
		wheelBL.forwardFriction = tempStruct;
		tempStruct = wheelBL.sidewaysFriction;
		tempStruct.stiffness = sideways;
		wheelBL.sidewaysFriction = tempStruct;
	}
	
	// FixedUpdate is called once per physics frame
	void FixedUpdate() {
		//front wheel steering
		wheelFL.steerAngle = Input.GetAxis("Horizontal") * maxTurnAngle;
		wheelFR.steerAngle = Input.GetAxis("Horizontal")* maxTurnAngle;
		
		//calculate max speed in KM/H (optimized calc)
		currentSpeed = wheelBL.radius*wheelBL.rpm*Mathf.PI*0.12f;
		if(currentSpeed < topSpeed && currentSpeed > maxReverseSpeed) {
			//rear wheel drive.
			wheelBL.motorTorque = Input.GetAxis("Vertical") * maxTorque;
			wheelBR.motorTorque = Input.GetAxis("Vertical") * maxTorque;
		} else {
			//can't go faster, already at top speed that engine produces.
			wheelBL.motorTorque = 0;
			wheelBR.motorTorque = 0;
		}
		
		//Spoilers add down pressure based on the car’s speed. (Upside-down lift)
		Vector3 localVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
		GetComponent<Rigidbody>().AddForce(-transform.up * (localVelocity.z * spoilerRatio),ForceMode.Impulse);
		
		//Handbrake controls
		if(Input.GetButton("Jump")) {
			applyHandbrake = true;
			wheelFL.brakeTorque = maxBrakeTorque;
			wheelFR.brakeTorque = maxBrakeTorque;
			
			if(GetComponent<Rigidbody>().velocity.magnitude > 1) {
				SetSlipValues(handbrakeForwardSlip, handbrakeSidewaysSlip);
			} else {
				SetSlipValues(1f,1f);
			}
		} else {
			applyHandbrake = false;
			wheelFL.brakeTorque = 0;
			wheelFR.brakeTorque = 0;
			SetSlipValues(1f,1f);
		}
		
		//apply deceleration when not pressing the gas or when breaking in either direction.
		if( !applyHandbrake && ((Input.GetAxis("Vertical") <= -0.5f && localVelocity.z > 0)||(Input.GetAxis("Vertical") >= 0.5f && localVelocity.z < 0))) {
			wheelBL.brakeTorque = decelerationTorque + maxTorque;
			wheelBR.brakeTorque = decelerationTorque + maxTorque;
		} else if(!applyHandbrake && Input.GetAxis("Vertical") == 0) {
			wheelBL.brakeTorque = decelerationTorque;
			wheelBR.brakeTorque = decelerationTorque;
		} else {
			wheelBL.brakeTorque = 0;
			wheelBR.brakeTorque = 0;
		}
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

		if(wheelBL.GetGroundHit(out contact)) {
			Vector3 temp = wheelBL.transform.position;
			temp.y = (contact.point + (wheelBL.transform.up*wheelBL.radius)).y;
			wheelTransformBL.position = temp;
		}

		if(wheelBR.GetGroundHit(out contact)) {
			Vector3 temp = wheelBR.transform.position;
			temp.y = (contact.point + (wheelBR.transform.up*wheelBR.radius)).y;
			wheelTransformBR.position = temp;
		}
	}
	
	void Update() {
		//rotate the wheels based on RPM
		float rotationThisFrame = 360*Time.deltaTime;
		wheelTransformFL.Rotate(wheelFL.rpm/rotationThisFrame,0,0);
		wheelTransformFR.Rotate(wheelFR.rpm/rotationThisFrame,0,0);
		wheelTransformBL.Rotate(wheelBL.rpm/rotationThisFrame,0,0);
		wheelTransformBR.Rotate(wheelBR.rpm/rotationThisFrame,0,0);
		
		//turn the wheels according to steering. But make sure you take into account the rotation being applied above.
		wheelTransformFL.localEulerAngles = new Vector3(wheelTransformFL.localEulerAngles.x, wheelFL.steerAngle - wheelTransformFL.localEulerAngles.z, wheelTransformFL.localEulerAngles.z);
		wheelTransformFR.localEulerAngles = new Vector3(wheelTransformFR.localEulerAngles.x, wheelFR.steerAngle - wheelTransformFR.localEulerAngles.z, wheelTransformFR.localEulerAngles.z);
		
		//Adjust the wheels heights based on the suspension.
		UpdateWheelPositions();
		
		//Determine what texture to use on our brake lights right now.
		DetermineBreakLightState();
		
		//adjust engine sound
		EngineSound();
	}
	
	void DetermineBreakLightState() {
		if((currentSpeed > 0 && Input.GetAxis("Vertical") < 0) 
			|| (currentSpeed < 0 && Input.GetAxis("Vertical") > 0)
			|| applyHandbrake) {
			leftBrakeLight.GetComponent<Renderer>().material.mainTexture = brakeLightTex;
			Light leftLight = leftBrakeLight.GetComponentInChildren<Light>();
			leftLight.color = Color.red;
			leftLight.intensity = 1;
			rightBrakeLight.GetComponent<Renderer>().material.mainTexture = brakeLightTex;
			Light rightLight = rightBrakeLight.GetComponentInChildren<Light>();
			rightLight.color = Color.red;
			rightLight.intensity = 1;
		} else if(currentSpeed < 0 && Input.GetAxis("Vertical") < 0) {
			leftBrakeLight.GetComponent<Renderer>().material.mainTexture = reverseLightTex;
			Light leftLight = leftBrakeLight.GetComponentInChildren<Light>();
			leftLight.color = Color.white;
			leftLight.intensity = 1;
			rightBrakeLight.GetComponent<Renderer>().material.mainTexture = reverseLightTex;
			Light rightLight = rightBrakeLight.GetComponentInChildren<Light>();
			rightLight.color = Color.white;
			rightLight.intensity = 1;
		} else {
			leftBrakeLight.GetComponent<Renderer>().material.mainTexture = idleLightTex;
			Light leftLight = leftBrakeLight.GetComponentInChildren<Light>();
			leftLight.color = Color.white;
			leftLight.intensity = 0;
			rightBrakeLight.GetComponent<Renderer>().material.mainTexture = idleLightTex;
			Light rightLight = rightBrakeLight.GetComponentInChildren<Light>();
			rightLight.color = Color.white;
			rightLight.intensity = 0;
			
		}
	}
	
	void EngineSound() {
		//going forward calculate how far along that gear we are and the pitch sound.
		if(currentSpeed > 0) {
			if(currentSpeed > topSpeed) {
				GetComponent<AudioSource>().pitch = 1.75f;
			} else {
				GetComponent<AudioSource>().pitch = ((currentSpeed % gearSpread) / gearSpread) + 0.75f;
			}
		} else { //when reversing we have only one gear.
			GetComponent<AudioSource>().pitch = (currentSpeed / maxReverseSpeed) + 0.75f;
		}
	}
	
	void OnGUI() {
		GUI.DrawTexture(new Rect(Screen.width-300,Screen.height-150,300,150),speedometer);
		float speedFactor=currentSpeed/topSpeed;
		float rotationAngle = Mathf.Lerp(0,180,Mathf.Abs(speedFactor));	
		GUIUtility.RotateAroundPivot(rotationAngle,new Vector2(Screen.width-150,Screen.height));
		GUI.DrawTexture(new Rect(Screen.width - 300, Screen.height - 150, 300, 300),needle);
	}
}
