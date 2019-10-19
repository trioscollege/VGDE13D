using UnityEngine;
using System.Collections;

public class WanderingMonster : MonoBehaviour 
{
	public float speed = 5;
	public float turningSpeed=1;
	public float maxHeadingChange = 1;
	public float gravity =9.81f;
	public float waitToCharge = 5f;
	public float attackRadius = 1f;
	public HitBox weaponHitBox;
	
	private float heading;
	private Vector3 targetRotation;
	public Transform target{get;set;}
	private int crawlState;
	private int runState;
	private int attackState;
	private Animator animController;
	private CharacterController controller;
	
	void Start ()
	{
		heading = Random.Range(0, 360);
		transform.eulerAngles = new Vector3(0, heading, 0);
		controller = GetComponent<CharacterController>();
		animController = GetComponent<Animator>();
		crawlState = Animator.StringToHash("Base.crawl"); 
		runState = Animator.StringToHash("Base.run");
		attackState = Animator.StringToHash("UpperTorso.attack");
		weaponHitBox.GetComponent<Collider>().enabled = false;
		
	}
	
	void Update ()
	{
		Vector3 moveDirection = Vector3.zero;
			
		//wander
		if(target == null)
		{
			transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, targetRotation, Time.deltaTime*turningSpeed);
			NewHeading();
						
			AnimatorStateInfo currentBaseState = animController.GetCurrentAnimatorStateInfo(0);
			if (currentBaseState.fullPathHash == crawlState)
			{
				moveDirection = transform.forward * speed;
			}
		}    
		//chase enemy
		else
		{
			// Calculate the direction from the current position to the target (ignoring height)
			Vector3 targetDirection = target.position;
			targetDirection.y = transform.position.y;
			targetDirection -= transform.position;
			
			// Calculate the rotation required to point at the target
			Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
			
			AnimatorStateInfo currentUpperTorsoState = animController.GetCurrentAnimatorStateInfo(1);
			if(targetDirection.magnitude < attackRadius && currentUpperTorsoState.fullPathHash != attackState)
			{				
				animController.SetBool("attacking",true);
				weaponHitBox.GetComponent<Collider>().enabled = true;
				waitToCharge=0;
			}
			else
			{
				animController.SetBool("attacking",false);				
				weaponHitBox.GetComponent<Collider>().enabled = false;
				
				if(waitToCharge > 0)
				{
					waitToCharge-=Time.deltaTime;
				}
				else
				{
					animController.SetBool("charging",true);
					
					//if we are standing and ready to run
					AnimatorStateInfo currentBaseState = animController.GetCurrentAnimatorStateInfo(0);
					if (currentBaseState.fullPathHash == runState)
					{
						// Rotate from the current rotation towards the target rotation.
						transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turningSpeed * Time.fixedTime);
						// Move forward
						moveDirection = transform.forward * speed;
					}
				}
			}
		}
		
		//Apply gravity
		moveDirection.y -= gravity;
		
		//Apply move
		controller.Move(moveDirection*Time.deltaTime);
	}
	
	void OnAnimatorIK()
	{
		if(target)
		{
			//Look at the target
			animController.SetLookAtPosition(target.position+ new Vector3(0,1,0));
			animController.SetLookAtWeight(1.0f);
		}
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
		animController.SetBool("hasTarget",true);
	}
}
