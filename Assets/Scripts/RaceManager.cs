using UnityEngine;
using System.Collections;

public class RaceManager : MonoBehaviour 
{
	public int requiredLaps =3;
	public Rigidbody player;
	public CheckPoint[] checkPoints;
	public Rigidbody[] cars;
	public float respawnDelay = 5f;
	public float distanceToCover = 1f;
	public Texture2D startRaceImage;
	public Texture2D digit1Image;
	public Texture2D digit2Image;
	public Texture2D digit3Image;
	private CarController[] scripts;
	private float[] respawnTimes;
	private float[] distanceLeftToTravel;
	private Transform[] waypoint;
	private int[] laps;
	private int currentCheckPoint = 0;
	private int playerLaps = 0;
	private int countdownTimerDelay;
	private float countdownTimerStartTime;
	private bool started = false;
	
	public static RaceManager Instance { get { return instance; } }
	
	private static RaceManager instance = null;
	
	void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(this.gameObject);
			return;        
		} 
		else 
		{
			instance = this;
		}
		CountdownTimerReset(5);
	}
	
	// Use this for initialization
	void Start () 
	{
		respawnTimes = new float[cars.Length];
		distanceLeftToTravel = new float[cars.Length];
		scripts = new CarController[cars.Length];
		waypoint = new Transform[cars.Length];
		laps = new int[cars.Length];
		
		//intialize the arrays with starting values
		for(int i=0; i < respawnTimes.Length; ++i)
		{
			scripts[i] = cars[i].gameObject.GetComponent<CarController>();
			respawnTimes[i] = respawnDelay;
			distanceLeftToTravel[i] = float.MaxValue;
			laps[i] = 0;
			
			//disable the cars until the race starts
			cars[i].isKinematic = true;
		}
		
		//disable the player until the race starts
		player.isKinematic = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//if the race has started...
		if(started)
		{
			//counter that tracks finished cars.
			int carsFinished=0;
			
			//check if any of the cars need a respawn.
		 	for(int i = 0; i < cars.Length; ++i)
			{
				Transform nextWaypoint = scripts[i].GetCurrentWaypoint();
				float distanceCovered = (nextWaypoint.position - cars[i].position).magnitude;
				
				//if the car has moved far enough or is now moving to a new waypoint reset its values.
				if(distanceLeftToTravel[i] - distanceToCover > distanceCovered || waypoint[i] != nextWaypoint)
				{
					waypoint[i] = nextWaypoint;
					respawnTimes[i] = respawnDelay;
					distanceLeftToTravel[i] = distanceCovered;
				}
				//otherwise tick down time before we respawn it.
				else
				{
					respawnTimes[i] -= Time.deltaTime;
				}
				
				//if it's respawn timer has elapsed.
				if(respawnTimes[i] <= 0)
				{
					//reset its respawn tracking variables
					respawnTimes[i] = respawnDelay;
					distanceLeftToTravel[i] = float.MaxValue;
					cars[i].velocity = Vector3.zero;
					//And spaw it at its last waypoint facing the next waypoint.
					Transform lastWaypoint = scripts[i].GetLastWaypoint();	
					cars[i].position = lastWaypoint.position;
					cars[i].rotation = Quaternion.LookRotation(nextWaypoint.position - lastWaypoint.position);
				}
				
				//count the cars that have finished the race.
				if(laps[i] >= requiredLaps)
				{
					carsFinished++;
				}
			}
			
			//if all the cars finish their laps or the player does load the race track.
			if(carsFinished >= cars.Length || playerLaps >= requiredLaps)
			{
				print ("Player placed: " + (carsFinished+1));
				Application.LoadLevel("RaceTrack");
			}
		}
	}
	
	public void LapFinishedByAI(CarController script)
	{
		//search through and find the car that communicated with us.
		for(int i=0; i < respawnTimes.Length; ++i)
		{
			if(scripts[i] == script)
			{
				//increment its lap counter
				laps[i]++;
				break;
			}
		}
	}
	
	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
    	GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(CountdownTimerImage());	
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();	
	}
	
	void CountdownTimerReset(int delayInSeconds)
	{
		countdownTimerDelay = delayInSeconds;
		countdownTimerStartTime = Time.time;
	}
	
	int CountdownTimerSecondsRemaining()
	{
		int elapsedSeconds = (int) (Time.time - countdownTimerStartTime);
		int secondsLeft = (countdownTimerDelay - elapsedSeconds);
		return secondsLeft;
	}
	
	Texture2D CountdownTimerImage()
	{
		switch(CountdownTimerSecondsRemaining())
		{
		case 3:
			return digit3Image;
			
		case 2:
			return digit2Image;
			
		case 1:
			return digit1Image;
			
		case 0:
			//start the race!
			for(int i=0; i < scripts.Length; ++i)
			{
				cars[i].isKinematic = false;
				player.isKinematic = false;
				started = true;
			}
			return startRaceImage;

		default:
			return null;
		}
	}
	
	public void PlayerCheckPoint(CheckPoint point)
	{
		if(point == checkPoints[currentCheckPoint])
		{
			currentCheckPoint++;
			if(currentCheckPoint >= checkPoints.Length)
			{
				currentCheckPoint = 0;
				playerLaps++;
			}
		}
	}
}
