using UnityEngine;
using System.Collections;

public class SkinSelector : MonoBehaviour 
{
	public float skinValue = 0.5f;
	public float skinTint = 0.05f;
	private Renderer rend;

	void Start()
	{
		rend = GetComponent<Renderer>();
	}

	void OnGUI()
	{
		GUI.Label(new Rect(510, 30, 90, 20), "Skin Shade:");
		skinValue = GUI.HorizontalSlider(new Rect(585, 35, 256,20), skinValue, 0.4f, 1.0f);
		GUI.Label(new Rect(510, 55, 90, 20), "Skin Tint:");
		skinTint = GUI.HorizontalSlider(new Rect(585, 60, 256,20), skinTint, 0f, 0.1f);
		if (GUI.Button(new Rect(550, 85, 80, 20), "Save Skin"))
		{
			PlayerPrefs.SetFloat("skinValue",skinValue);
			PlayerPrefs.SetFloat("skinTint",skinTint);
		}
		
		rend.material.SetColor("_Color", new Color(skinValue+skinTint, skinValue+(skinTint+skinTint), skinValue, 1));
	}
}
