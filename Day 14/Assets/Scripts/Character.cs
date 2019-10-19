using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour 
{
	public int maxHitPoints = 100;
	public float deathTime = 3f;
	public float hitReact = 0.1f;
	private HealthBar healthBar;
	private HealthMeter healthMeter;
	private int currentHealth;
	private Animator animController;
	private float hitDelay;
	private Transform agroTarget;
	
	// Use this for initialization
	void Start () 
	{
		animController = GetComponent<Animator>();
		currentHealth = maxHitPoints;
		
		healthBar = GetComponent<HealthBar>();
		if(healthBar != null)
		{
			healthBar.SetHealth(currentHealth);	
		}
		
		healthMeter = GetComponent<HealthMeter>();
		if(healthMeter != null)
		{
			healthMeter.SetMaxHitPoints(currentHealth);	
		}
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
		
		if(healthBar != null)
		{
			healthBar.AlterHealth(-amount);	
		}
		
		if(healthMeter != null)
		{
			healthMeter.AlterHealth(-amount);	
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
