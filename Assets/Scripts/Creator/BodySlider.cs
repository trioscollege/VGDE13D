using UnityEngine;
using System.Collections;

public class BodySlider : MonoBehaviour 
{
	public float startPoint = 0.5f;
	public float minHeightValue = 0.8f;
	public float maxHeightValue = 1.2f;
	public float minWidthValue = 0.8f;
	public float maxWidthValue = 1.2f;
	public float minBulkValue = -0.5f;
	public float maxBulkValue = 0.5f;
	public Transform target;
	private float bulkValue;
	private float heightValue;
	private float widthValue;
	private Renderer rend;
	
	// Use this for initialization
	void Start () 
	{
		rend = GetComponent<Renderer>();

		//set initial values to the starting point set by design.
		bulkValue = minBulkValue+((maxBulkValue - minBulkValue)*startPoint);
		heightValue = minHeightValue+((maxHeightValue - minHeightValue)*startPoint);
		widthValue = minWidthValue+((maxWidthValue - minWidthValue)*startPoint);
	}
	
	// Update is called once per frame
	void OnGUI () 
	{
		//Height
		GUI.Label(new Rect(10, 10, 90, 20), "Height:");
		heightValue = GUI.HorizontalSlider(new Rect(10, 25, 256,20), heightValue, minHeightValue, maxHeightValue);
		
		//Width
		GUI.Label(new Rect(10, 40, 90, 20), "Width:");
		widthValue = GUI.HorizontalSlider(new Rect(10, 55, 256,20), widthValue, minWidthValue, maxWidthValue);
		
		//apply the size change to the model.
  	  	target.localScale = new Vector3(widthValue, heightValue, target.localScale.z);

		//Bulk
		GUI.Label(new Rect(10, 70, 90, 20), "Bulk:");
		bulkValue = GUI.HorizontalSlider(new Rect(10, 85, 256,20), bulkValue, minBulkValue, maxBulkValue);
		rend.material.SetFloat("_Amount",  bulkValue);
	}
}
