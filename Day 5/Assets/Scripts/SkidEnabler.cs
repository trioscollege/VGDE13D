using UnityEngine;
using System.Collections;

public class SkidEnabler : MonoBehaviour 
{
	public WheelCollider wheelCollider;
	public GameObject skidTrailRenderer;
	public float skidLife = 4f;
	private TrailRenderer skidMark;

	void Start () 
	{
		skidMark = skidTrailRenderer.GetComponent<TrailRenderer>();
		//this avoids a visual bug on first use, if the art team set the effect’s time to 0.
		skidMark.time = skidLife;
	}

	void Update () 
	{
		if(wheelCollider.forwardFriction.stiffness < 1 && wheelCollider.isGrounded)
		{
			if(skidMark.time == 0)
			{
				skidMark.time = skidLife;
				skidTrailRenderer.transform.parent = wheelCollider.transform;
				skidTrailRenderer.transform.localPosition = wheelCollider.center + ((wheelCollider.radius-0.1f) * -wheelCollider.transform.up);
			}
			
			if(skidTrailRenderer.transform.parent == null)
			{
				skidMark.time = 0;
			}
		}
		else
		{
			skidTrailRenderer.transform.parent = null;
		}	
	}
}
