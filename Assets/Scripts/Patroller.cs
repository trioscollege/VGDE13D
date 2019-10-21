using UnityEngine;
using System.Collections;

public class Patroller : MonoBehaviour 
{
	public Transform[] route;
	public float attackRadius = 2.1f;
	public Collider weaponHitBox;
	private Animator animController;
	private UnityEngine.AI.NavMeshAgent agent;
	private int currentPoint = 0;
	private const float MIN_DISTANCE=3;
	private Character character;
	private int attackState;
	private int deadState;
	
	
	// Use this for initialization
	void Start () 
	{
		agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); 
		character = GetComponent<Character>();
		agent.SetDestination(route[currentPoint].position);
		animController = GetComponent<Animator>();
		attackState = Animator.StringToHash("UpperTorso.attack");
		deadState = Animator.StringToHash("Base.die");
		weaponHitBox.enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		AnimatorStateInfo currentBaseState = animController.GetCurrentAnimatorStateInfo(0);
		if(currentBaseState.fullPathHash != deadState)
		{
			Transform target = character.GetAgroTarget();
			if(target != null)
			{
				agent.SetDestination(target.position);
				Vector3 targetDirection = transform.position - target.position;
				AnimatorStateInfo currentUpperTorsoState = animController.GetCurrentAnimatorStateInfo(1);
				if(targetDirection.magnitude < attackRadius)
				{				
					if(currentUpperTorsoState.fullPathHash == attackState)
					{
						animController.SetBool("attacking",false);
						weaponHitBox.enabled = true;
					}
					else
					{
						animController.SetBool("attacking",true);
						weaponHitBox.enabled = false;
					}
				}
				else
				{
					animController.SetBool("attacking",false);				
					weaponHitBox.enabled = false;
				}
			}
			else if((transform.position - route[currentPoint].position).magnitude < MIN_DISTANCE)
			{	
				++currentPoint;
				
				if(currentPoint >= route.Length)
				{
					currentPoint = 0;
				}
				
				agent.SetDestination(route[currentPoint].position);
			}
		}
	}
}
