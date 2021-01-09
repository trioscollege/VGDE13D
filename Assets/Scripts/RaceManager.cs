using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public Rigidbody m_player;
    public Rigidbody[] m_aiRacers;
    public int m_requiredLaps = 3;
    public float m_respawnDelay = 5.0f;
    public float m_distanceToCover = 1.0f;
    public GameObject m_checkpointContainer;

    private AICar_old[] m_aiControllers;
    private float[] m_respawnTimes;
    private float[] m_distancesLeftToTravel;
    private Transform[] m_waypoints;
    private CheckpointTrigger[] m_checkpoints;
    private int m_currentCheckpoint = 0;
    private int[] m_aiLaps;
    private int m_playerLaps = 0;
    private int m_carsFinished = 0;

    // race timer
    private Texture2D[] m_counterImages;
    private int m_countdownDelay;
    private float m_countdownStart;
    private bool m_started = false;

    public static RaceManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        CountdownReset(5);
        LoadResources();
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeArrays();
        DisableRacers();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_started)
        {
            for (int i = 0; i < m_aiRacers.Length; i++)
            {
                Transform nextWaypoint = m_aiControllers[i].CurrentWaypoint;
                float distanceToWaypoint = (nextWaypoint.position - m_aiRacers[i].position).magnitude;

                if (m_distancesLeftToTravel[i] - m_distanceToCover > distanceToWaypoint ||
                    m_waypoints[i] != nextWaypoint)
                {
                    m_waypoints[i] = nextWaypoint;
                    m_respawnTimes[i] = m_respawnDelay;
                    m_distancesLeftToTravel[i] = distanceToWaypoint;
                }
                else
                {
                    m_respawnTimes[i] -= Time.deltaTime;
                }

                if (m_respawnTimes[i] <= 0)
                {
                    m_respawnTimes[i] = m_respawnDelay;
                    m_distancesLeftToTravel[i] = float.MaxValue;
                    m_aiRacers[i].velocity = Vector3.zero;
                    m_aiRacers[i].angularVelocity = Vector3.zero;

                    Transform lastWaypoint = m_aiControllers[i].LastWaypoint;
                    m_aiRacers[i].position = lastWaypoint.position;
                    m_aiRacers[i].rotation = Quaternion.LookRotation(nextWaypoint.position - lastWaypoint.position);
                }

                if (m_aiLaps[i] == m_requiredLaps && !m_aiControllers[i].Finished)
                {
                    Debug.Log(m_aiRacers[i].name + " placed " + (m_carsFinished + 1));
                    m_aiControllers[i].Finished = true;
                    m_carsFinished += 1;
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                int resetCheckpoint = m_currentCheckpoint - 1;
                if (resetCheckpoint < 0)
                {
                    resetCheckpoint = m_checkpoints.Length - 1;
                }

                Transform resetLocation = m_checkpoints[resetCheckpoint].transform;
                m_player.velocity = Vector3.zero;
                m_player.angularVelocity = Vector3.zero;
                m_player.position = resetLocation.position;
                m_player.rotation = resetLocation.rotation;
            }

            if ((m_carsFinished == m_aiRacers.Length || m_playerLaps == m_requiredLaps) && !m_player.GetComponent<PlayerCar_old>().Finished)
            {
                m_player.gameObject.GetComponent<PlayerCar_old>().Finished = true;
                m_player.gameObject.GetComponent<PlayerCar_old>().enabled = false;
                //m_player.gameObject.GetComponent<AICar>().enabled = true;
                //m_player.gameObject.GetComponent<AICar>().Finished = true;
                Debug.Log("Player placed: " + (m_carsFinished + 1));
                m_carsFinished += 1;
            }
        }
    }

    public void LapCompletedByAI(AICar_old car)
    {
        bool found = false;
        for (int i = 0; i < m_aiControllers.Length && !found; i++)
        {
            if (m_aiControllers[i] == car)
            {
                if (!m_aiControllers[i].Finished)
                {
                    m_aiLaps[i] += 1;
                    Debug.Log("Lap completed by " + car.name);
                }
                found = true;
            }
        }
    }

    public void PlayerPassedCheckpoint(CheckpointTrigger point)
    {
        if (point == m_checkpoints[m_currentCheckpoint])
        {
            m_currentCheckpoint += 1;
            if (m_currentCheckpoint == m_checkpoints.Length)
            {
                m_currentCheckpoint = 0;
                m_playerLaps += 1;
                Debug.Log("Lap completed by Player");
            }
        }
    }

    void OnGUI()
    {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(CountdownImage());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
    }

    private void CountdownReset(int delayInSeconds)
    {
        m_countdownDelay = delayInSeconds;
        m_countdownStart = Time.time;
    }

    private int CountdownSecondsRemaining()
    {
        int elapsedSeconds = (int)(Time.time - m_countdownStart);
        int secondsLeft = m_countdownDelay - elapsedSeconds;
        return secondsLeft;
    }

    private Texture2D CountdownImage()
    {
        switch (CountdownSecondsRemaining())
        {
            case 3:
                return m_counterImages[3];
            case 2:
                return m_counterImages[2];
            case 1:
                return m_counterImages[1];
            case 0:
                EnableRacers();
                m_started = true;
                return m_counterImages[0];
            default:
                return null;
        }
    }

    private void EnableRacers()
    {
        foreach (Rigidbody rb in m_aiRacers)
        {
            rb.isKinematic = false;
        }
        m_player.isKinematic = false;
    }

    private void DisableRacers()
    {
        foreach (Rigidbody rb in m_aiRacers)
        {
            rb.isKinematic = true;
        }
        m_player.isKinematic = true;
    }

    private void LoadResources()
    {
        m_counterImages = new Texture2D[4];
        m_counterImages[0] = Resources.Load("GUI/go") as Texture2D;
        m_counterImages[1] = Resources.Load("GUI/one") as Texture2D;
        m_counterImages[2] = Resources.Load("GUI/two") as Texture2D;
        m_counterImages[3] = Resources.Load("GUI/three") as Texture2D;
    }

    private void InitializeArrays()
    {
        m_aiControllers = new AICar_old[m_aiRacers.Length];
        m_distancesLeftToTravel = new float[m_aiRacers.Length];
        m_waypoints = new Transform[m_aiRacers.Length];
        m_checkpoints = m_checkpointContainer.GetComponentsInChildren<CheckpointTrigger>();
        m_respawnTimes = new float[m_aiRacers.Length];
        m_aiLaps = new int[m_aiRacers.Length];

        for (int i = 0; i < m_aiRacers.Length; i++)
        {
            m_aiControllers[i] = m_aiRacers[i].GetComponentInChildren<AICar_old>();
            m_respawnTimes[i] = m_respawnDelay;
            m_distancesLeftToTravel[i] = float.MaxValue;
            m_aiLaps[i] = 0;
        }
    }
}
