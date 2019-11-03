using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour 
{
	public Transform target;
	private UnityEngine.AI.NavMeshAgent agent;

	
	// Use this for initialization
	void Start () 
	{
		agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		agent.SetDestination(target.position);
	}
}
