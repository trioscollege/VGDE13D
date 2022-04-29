using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour {
    public AICar[] aiCars;
    public float respawnDelay = 5f;
    public float distanceToCover = 1f;
    private Rigidbody[] aiBodies;
    private float[] respawnCounters;
    private float[] distancesLeft;
    private Transform[] waypoints;
    private int[] laps;

    public static RaceManager Instance { get; private set; } = null;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        }
        else {
            Instance = this;
        }
    }

    void Start() {
        aiBodies = new Rigidbody[aiCars.Length];
        respawnCounters = new float[aiCars.Length];
        distancesLeft = new float[aiCars.Length];
        waypoints = new Transform[aiCars.Length];
        laps = new int[aiCars.Length];

        for (int i = 0; i < aiCars.Length; i++) {
            aiBodies[i] = aiCars[i].gameObject.GetComponent<Rigidbody>();
            respawnCounters[i] = respawnDelay;
            distancesLeft[i] = float.MaxValue;
            laps[i] = 0;
        }
    }

    void Update() {
        for (int i = 0; i < aiBodies.Length; i++) {
            Transform nextWaypoint = aiCars[i].CurrentWaypoint;
            float distanceCovered = (nextWaypoint.position - aiBodies[i].position).magnitude;
        
            if (distancesLeft[i] - distanceToCover > distanceCovered || waypoints[i] != nextWaypoint) {
                waypoints[i] = nextWaypoint;
                respawnCounters[i] = respawnDelay;
                distancesLeft[i] = distanceCovered;
            } 
            else {
                respawnCounters[i] -= Time.deltaTime;

                if (respawnCounters[i] <= 0) {
                    respawnCounters[i] = respawnDelay;
                    distancesLeft[i] = float.MaxValue;
                    aiBodies[i].velocity = Vector3.zero;
                    Transform lastWaypoint = aiCars[i].LastWaypoint;
                    aiBodies[i].position = lastWaypoint.position;
                    aiBodies[i].rotation = Quaternion.LookRotation(nextWaypoint.position - lastWaypoint.position);
                }
            }

            if (laps[i] == 3) {
                SceneManager.LoadScene("Track1");
            }
        }
    }

    public void LapFinishedByAI(AICar car) {
        int i = Array.FindIndex(aiCars, element => element == car);
        if (i != -1) {
            laps[i] += 1;
        }
    }
}
