using UnityEngine;
using System.Collections;

public class CheckPoint : MonoBehaviour 
{
	void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			RaceManager.Instance.PlayerCheckPoint(this);
		}
	}
}
