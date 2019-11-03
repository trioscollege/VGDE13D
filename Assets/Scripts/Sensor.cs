using UnityEngine;
using System.Collections;

public class Sensor : MonoBehaviour 
{
	public WanderingMonster brainScript;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player")
		{
			brainScript.target = other.transform;	
		}
	}
}
