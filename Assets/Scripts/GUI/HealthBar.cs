using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour
{
	public Texture icon;
	public Vector2 startPoint = Vector2.one;
	public int maxIconsPerRow = 5;
	private int healthLeft = 25;
	
	//display the health bar.
	private void OnGUI()
	{
		int icons = healthLeft;
		Rect rect = new Rect(startPoint.x,startPoint.y,Screen.width - startPoint.x, Screen.height-startPoint.y);
		GUILayout.BeginArea(rect);
		do
		{
			//print a row of icons up to the max allowed or the health left, whichever is less.
			GUILayout.BeginHorizontal();
			
			//if there are more icons left than we can fit on a row make a full row.
			if(icons > maxIconsPerRow)
			{
				ImagesForInteger(maxIconsPerRow);
			}
			//else create a row with just the remainder
			else
			{
				ImagesForInteger(icons);
			}
			
			//fill in the rest of the screen with blank space.
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			//decrement our icon counter
			icons -= maxIconsPerRow;
		}while(icons>0);
		
		GUILayout.EndArea();
	}
	
	//display a certain amount of icons
	private void ImagesForInteger(int total)
	{
		for(int i=0; i < total; i++)
		{
			GUILayout.Label(icon);
		}
	}
	
	//set the health bar to a specific value.
	public void SetHealth(int hitPoints)
	{
		healthLeft = hitPoints;
	}
	
	//alter the health bar by a certain amount.
	public void AlterHealth(int amount)
	{
		healthLeft += amount;
	}
}