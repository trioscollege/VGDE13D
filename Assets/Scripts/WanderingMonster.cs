using UnityEngine;
using System.Collections;

public class WanderingMonster : MonoBehaviour 
{
	public float speed = 5;
	public float turningSpeed = 1;
	public float maxHeadingChange = 1;
	public float gravity = 9.81f;
	
	private float heading;
	private Vector3 targetRotation;
	public Transform target{get;set;}
	private int crawlState; 
	private Animator animController;
	private CharacterController controller;
	
	void Start ()
	{
		heading = Random.Range(0, 360);
		transform.eulerAngles = new Vector3(0, heading, 0);
		controller = GetComponent<CharacterController>();
		animController = GetComponent<Animator>();
		crawlState = Animator.StringToHash("Base.crawl"); 
	}
	
	void Update ()
	{
		Vector3 moveDirection = Vector3.zero;
			
		//wander
		if(target == null) {
			transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, targetRotation, Time.deltaTime*turningSpeed);
			NewHeading();
						
			AnimatorStateInfo currentBaseState = animController.GetCurrentAnimatorStateInfo(0);
			if (currentBaseState.fullPathHash == crawlState) {
				moveDirection = transform.forward * speed;
			}
		//chase enemy
		} else { 
			// Calculate the direction from the current position to the target
			Vector3 targetDirection = target.position - transform.position;
			// Calculate the rotation required to point at the target
			Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
			// Rotate from the current rotation towards the target rotation, but not
			// faster than mRotationSpeed degrees per second
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turningSpeed * Time.fixedTime);
			// Move forward
			moveDirection = transform.forward * speed;
		}
		
		//Apply gravity
		moveDirection.y -= gravity;
		
		//Apply move
		controller.Move(moveDirection*Time.deltaTime);
	}


	void NewHeading()
	{
		float floor = Mathf.Clamp(heading - maxHeadingChange, 0, 360);
	    float ceil  = Mathf.Clamp(heading + maxHeadingChange, 0, 360);
	    heading = Random.Range(floor, ceil);
	    targetRotation = new Vector3(0, heading, 0);
    }
	
	public void SpottedEnemy(Transform enemy)
	{
		//target aquired...
		target = enemy;
	}
}
