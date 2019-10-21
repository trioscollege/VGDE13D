using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour 
{
	public int maxHitPoints = 100;
	public float deathTime = 3f;
	public float hitReact = 0.1f;
	private int currentHealth;
	private Animator animController;
	private float hitDelay;
	private Transform agroTarget;
	
	// Use this for initialization
	void Start () 
	{
		animController = GetComponent<Animator>();
		currentHealth = maxHitPoints;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(hitDelay<=0)
		{
			animController.SetBool("tookDamage",false);
		}
		else
		{
			hitDelay-=Time.deltaTime;	
		}
		
		if(currentHealth <= 0)
		{
			Die();	
		}
	}
	
	//The character has been hurt by something so apply the damage.
	public void ApplyDamage(int amount)
	{
		
		currentHealth -= amount;
		if(currentHealth <= 0)
		{
			hitDelay=deathTime;
			animController.SetBool("died",true);
		}
		else
		{
			hitDelay=hitReact;
			animController.SetBool("tookDamage",true);
		}
	}
	
	private void Die()
	{
		if(hitDelay<=0)
		{
			Destroy(gameObject);
		}
	}
	
	public Transform GetAgroTarget()
	{
		return agroTarget ? agroTarget : null;	
	}
	
	public void SetAgroTarget(Transform target)
	{
		agroTarget = target;
	}
	
}
