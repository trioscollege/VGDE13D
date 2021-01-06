using UnityEngine;

public class AICar : CarController
{
    public Transform m_waypointContainer;
    [Tooltip("The distance, in units, a car must be to a waypoint to 'arrive'.")]
    public float m_waypointProximity = 15.0f;
    public float m_sideRayOffset = 1.0f;
    public float m_forwardRayOffset = 1.0f;
    public float m_rayHeightOffset = 0.25f;
    public float m_avoidDistance = 2.0f;
    public float m_brakingDistance = 2.0f;
    private Transform[] m_waypoints;
    private int m_currentWaypoint = 0;
    private float m_inputSteer;
    private float m_inputTorque;

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

    // Start is called before the first frame update
    void Start()
    {
        GetWaypoints();
    }

    void FixedUpdate()
    {
        Vector3 waypointPosition = m_waypoints[m_currentWaypoint].position;
        Vector3 relativeWaypointPos = transform.InverseTransformPoint(new Vector3(waypointPosition.x, transform.position.y, waypointPosition.z));

        // steering amount based on left / right offset of waypoint
        m_inputSteer = relativeWaypointPos.x / relativeWaypointPos.magnitude;
        // avoid grinding doors
        m_inputSteer += CheckSpacing();
        // avoid clipping bumpers
        m_inputSteer += CheckOnComing();

        // spoiler down force
        Vector3 localVelocity = transform.InverseTransformDirection(m_body.velocity);
        m_body.AddForce(-transform.up * (localVelocity.z * m_spoilerRatio), ForceMode.Force);

        // acceleration magnitude depends on how much turning is required - less steering = more accelerating
        m_inputTorque = 1.0f - Mathf.Abs(m_inputSteer);

        // waypoint is behind vehicle
        if (relativeWaypointPos.z < 0)
        {
            // turn wheels opposite for reversing
            m_inputSteer *= -1;
            // reverse
            m_inputTorque = -1.0f;
        }

        // no acceleration input
        if (m_inputTorque == 0)
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

        // steering - front colliders only, indicies 0 and 1
        for (int i = 0; i < 2; i++)
        {
            m_wheelColliders[i].steerAngle = m_inputSteer * m_maxSteerAngle;
        }

        // sample rear left tire speed, index 2 - calculate KM/H from RPM (assume a radius 10x bigger)
        m_currentSpeed = ((20 * Mathf.PI) / 60.0f) * m_wheelColliders[2].radius * m_wheelColliders[2].rpm;
        if (m_currentSpeed < m_maxSpeed)
        {
            // accelerating - front colliders only, indicies 0 and 1
            for (int i = 0; i < 2; i++)
            {
                m_wheelColliders[i].motorTorque = m_inputTorque * m_maxSpeed;
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                m_wheelColliders[i].motorTorque = 0;
            }
        }

        float brakingMultiplier = CheckBraking();
        m_braking = brakingMultiplier > 0;
        foreach(WheelCollider wc in m_wheelColliders)
        {
            wc.brakeTorque = (m_brakingTorque + m_maxTorque) * brakingMultiplier;
        }

        if (Mathf.Abs(m_inputSteer) > 0.5f && m_inputTorque > 0.8f)
        {
            m_handBraking = true;
            foreach (WheelCollider wc in m_wheelColliders)
            {
                wc.brakeTorque = m_brakingTorque + m_maxTorque;
            }
            SetSlipValues(m_handbrakeFowardSlip, m_handbrakeSidewaysSlip);
        }
        else
        {
            m_handBraking = false;
            SetSlipValues(1.0f, 1.0f);
        }

        CheckWaypointPosition(relativeWaypointPos);
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

    private void CheckWaypointPosition(Vector3 relativeWaypointPos)
    {
        if (relativeWaypointPos.sqrMagnitude < m_waypointProximity * m_waypointProximity)
        {
            m_currentWaypoint += 1;

            if (m_currentWaypoint == m_waypoints.Length)
            {
                m_currentWaypoint = 0;
                RaceManager.Instance.LapCompletedByAI(this);
            }
        }
    }

    private float CheckBraking()
    {
        float brakeMultiplier = 0.0f;

        RaycastHit hit;
        Vector3 frontSide = transform.position + (transform.forward * m_forwardRayOffset) + (transform.up * m_rayHeightOffset);
        Debug.DrawRay(frontSide, (transform.forward * m_brakingDistance));

        if (Physics.Raycast(frontSide, transform.forward, out hit, m_brakingDistance))
        {
            brakeMultiplier = 1.0f - ((frontSide - hit.point).magnitude / m_brakingDistance);
        }

        return brakeMultiplier;
    }

    private float CheckSpacing()
    {
        float steeringAdjustment = 0.0f;

        RaycastHit hit;

        // check right side
        Vector3 rightSide = transform.position + (transform.right * m_sideRayOffset) + (transform.up * m_rayHeightOffset);
        Debug.DrawRay(rightSide, transform.right * m_avoidDistance);
        if (Physics.Raycast(rightSide, transform.right, out hit, m_avoidDistance))
        {
            steeringAdjustment += -1.0f + (rightSide - hit.point).magnitude / m_avoidDistance;
        }

        // check left side
        Vector3 leftSide = transform.position + (-transform.right * m_sideRayOffset) + (transform.up * m_rayHeightOffset);
        Debug.DrawRay(leftSide, -transform.right * m_avoidDistance);
        if (Physics.Raycast(leftSide, -transform.right, out hit, m_avoidDistance))
        {
            steeringAdjustment += 1.0f - (leftSide - hit.point).magnitude / m_avoidDistance;
        }
        return steeringAdjustment;
    }

    private float CheckOnComing()
    {
        float steeringAdjustment = 0.0f;

        RaycastHit hit;

        // check forward-right
        Vector3 forwardRight = transform.position + (transform.right * m_sideRayOffset) + (transform.forward * m_forwardRayOffset) + (transform.up * m_rayHeightOffset);
        Debug.DrawRay(forwardRight, (transform.right * 0.5f + transform.forward).normalized * m_avoidDistance);
        if (Physics.Raycast(forwardRight, (transform.right * 0.5f + transform.forward).normalized, out hit, m_avoidDistance))
        {
            steeringAdjustment += -1.0f + (forwardRight - hit.point).magnitude / m_avoidDistance;
        }

        // check forward-left
        Vector3 forwardLeft = transform.position + (-transform.right * m_sideRayOffset) + (transform.forward * m_forwardRayOffset) + (transform.up * m_rayHeightOffset);
        Debug.DrawRay(forwardLeft, (-transform.right * 0.5f + transform.forward).normalized * m_avoidDistance);
        if (Physics.Raycast(forwardLeft, (-transform.right * 0.5f + transform.forward).normalized, out hit, m_avoidDistance))
        {
            steeringAdjustment += 1.0f - (forwardLeft - hit.point).magnitude / m_avoidDistance;
        }

        return steeringAdjustment;
    }
}
