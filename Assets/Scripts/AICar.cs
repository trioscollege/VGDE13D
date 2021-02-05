using UnityEngine;

public class AICar : CarController
{
    public GameObject m_waypointContainer;
    public float m_waypointProximity = 15.0f;
    private Transform[] m_waypoints;
    private int m_currentWaypoint = 0;

    public Transform CurrentWaypoint { get { return m_waypoints[m_currentWaypoint]; } }
    public Transform LastWaypoint
    {
        get
        {
            if (m_currentWaypoint - 1 < 0)
            {
                return m_waypoints[m_waypoints.Length - 1];
            }
            return m_waypoints[m_currentWaypoint - 1];
        }
    }

    private void GetWaypoints()
    {
        Transform[] potentialWaypoints = m_waypointContainer.GetComponentsInChildren<Transform>();
        m_waypoints = new Transform[potentialWaypoints.Length - 1];

        for (int i = 1; i < potentialWaypoints.Length; i++)
        {
            m_waypoints[i - 1] = potentialWaypoints[i];
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GetWaypoints();
    }

    // Update is called once per frame
    new void Update()
    {
        Vector3 waypointPosition = m_waypoints[m_currentWaypoint].position;
        Vector3 relativeWaypointPos = transform.InverseTransformPoint(new Vector3(waypointPosition.x, transform.position.y, waypointPosition.z));
        Vector3 localVelocity = transform.InverseTransformDirection(m_body.velocity);

        CurrentSpeed = localVelocity.z * 3.6f;
        // steering amount based on left / right offset of waypoint
        SteeringInput = relativeWaypointPos.x / relativeWaypointPos.magnitude;
        // acceleration depends on distance to waypoint and steering
        AccelerationInput = relativeWaypointPos.normalized.magnitude - Mathf.Abs(SteeringInput);

        // waypoint is behind vehicle
        if (relativeWaypointPos.z < 0)
        {
            // turn wheels opposite for reversing
            SteeringInput *= -1;
            // reverse
            AccelerationInput = -1.0f;
        }

        // limit speed
        if (CurrentSpeed < m_maxSpeed)
        {
            // acceleration - front colliders only, indicies 0 and 1
            for (int i = 0; i < 2; i++)
            {
                m_wheelColliders[i].motorTorque = AccelerationInput * m_maxTorque;
            }
        }
        else
        {
            // too fast, coast
            for (int i = 0; i < 2; i++)
            {
                m_wheelColliders[i].motorTorque = 0;
            }
        }

        // steering - front colliders only, indicies 0 and 1
        for (int i = 0; i < 2; i++)
        {
            m_wheelColliders[i].steerAngle = SteeringInput * m_maxSteerAngle;
        }

        // spoiler down force
        m_body.AddForce(-transform.up * (localVelocity.z * m_spoilerRatio), ForceMode.Force);

        // no acceleration input
        if (AccelerationInput == 0)
        {
            for (int i = 0; i < 2; i++)
            {
                m_wheelColliders[i].brakeTorque = m_brakingTorque;
            }
            m_braking = false;
            m_reversing = false;
        }
        // accelerating
        else
        {
            for (int i = 0; i < 2; i++)
            {
                m_wheelColliders[i].brakeTorque = 0;
            }
            m_braking = false;
            m_reversing = localVelocity.z < 0;
        }

        CheckWaypointPosition(relativeWaypointPos);
        base.Update();
    }

    private void CheckWaypointPosition(Vector3 relativeWaypointPos)
    {
        if (relativeWaypointPos.sqrMagnitude < m_waypointProximity * m_waypointProximity)
        {
            m_currentWaypoint += 1;

            if (m_currentWaypoint == m_waypoints.Length)
            {
                m_currentWaypoint = 0;
            }
        }
    }
}
