using UnityEngine;

public class AICar : CarController
{
    public GameObject m_waypointContainer;
    public float m_waypointProximity = 15.0f;
    public float brakingDistance = 6f;
    public float brakingOffset = 2f;
    public float spacingDistance = 2f;
    public float spacingOffset = 1f;
    private float m_proximitySqr;
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
    void Start() {
        GetWaypoints();
        m_proximitySqr = m_waypointProximity * m_waypointProximity;
    }

    // Update is called once per frame
    new void Update() {
        Vector3 waypointPosition = m_waypoints[m_currentWaypoint].position;
        Vector3 relativeWaypointPos = transform.InverseTransformPoint(new Vector3(waypointPosition.x, transform.position.y, waypointPosition.z));
        Vector3 localVelocity = transform.InverseTransformDirection(m_body.velocity);
        float spacingAdjustment = SideCrashSteeringAdjustment();
        float brakingAdjustment = ForwardCrashBrakeAdjustment();

        CurrentSpeed = localVelocity.z * 3.6f;
        SteeringInput = relativeWaypointPos.x / relativeWaypointPos.magnitude + spacingAdjustment;
        AccelerationInput = (relativeWaypointPos.normalized.magnitude - Mathf.Abs(SteeringInput)) * brakingAdjustment;

        if (relativeWaypointPos.z < 0) {
            SteeringInput *= -1;
            AccelerationInput = -1.0f;
        }

        if (CurrentSpeed < m_maxSpeed) {
            for (int i = 0; i < 2; i++) {
                m_wheelColliders[i].motorTorque = AccelerationInput * m_maxTorque;
            }
        }
        else {
            for (int i = 0; i < 2; i++) {
                m_wheelColliders[i].motorTorque = 0;
            }
        }
        
        for (int i = 0; i < 2; i++) {
            m_wheelColliders[i].steerAngle = SteeringInput * m_maxSteerAngle;
        }

        m_body.AddForce(-transform.up * (localVelocity.z * m_spoilerRatio), ForceMode.Force);
        // decelerating / braking
        bool brakeTorqueChanged;
        if ((AccelerationInput < 0 && localVelocity.z > 0) ||
            (AccelerationInput > 0 && localVelocity.z < 0)) {
            
            m_appliedBrakeTorque = m_brakingTorque + m_maxTorque;
            brakeTorqueChanged = true;
            m_braking = true;
        }
        // no acceleration input
        else if (AccelerationInput == 0)
        {
            m_appliedBrakeTorque = m_brakingTorque;
            brakeTorqueChanged = true;
            m_braking = false;
        }
        else {
            m_appliedBrakeTorque = 0.0f;
            brakeTorqueChanged = true;
            m_braking = false;
            m_reversing = localVelocity.z < 0;
        }

        if (brakeTorqueChanged) {
            for (int i = 0; i < m_wheelColliders.Length; i++)
            {
                m_wheelColliders[i].brakeTorque = m_appliedBrakeTorque;
            }
        }

        CheckWaypointPosition(relativeWaypointPos);
        base.Update();
    }

    private void CheckWaypointPosition(Vector3 relativeWaypointPos)
    {
        if (relativeWaypointPos.sqrMagnitude < m_proximitySqr) {
            m_currentWaypoint += 1;

            if (m_currentWaypoint == m_waypoints.Length) {
                m_currentWaypoint = 0;
                RaceManager.Instance.LapFinishedByAI(this);
            }
        }
    }

    float ForwardCrashBrakeAdjustment() {
        Vector3 origin = transform.GetComponentInChildren<Renderer>().bounds.center;
        Vector3 rayStart = origin + (transform.forward * brakingOffset);       
        Debug.DrawRay(rayStart, transform.forward * brakingDistance);

        if (Physics.Raycast(rayStart, transform.forward, out RaycastHit hit, brakingDistance)) {
            return ((rayStart - hit.point).magnitude / brakingDistance * 2f) - 1f;
        }
        else {
            return 1f;
        }
    }

    float SideCrashSteeringAdjustment() {
        float steerAdjust = 0;
        Vector3 origin = transform.GetComponentInChildren<Renderer>().bounds.center;
        Vector3 carRightSide = origin + (transform.right * spacingOffset);
        Vector3 carLeftside = origin + (-transform.right * spacingOffset);
        Debug.DrawRay(carRightSide, transform.right * spacingDistance);
        Debug.DrawRay(carLeftside, -transform.right * spacingDistance);

        if (Physics.Raycast(carRightSide, transform.right, out RaycastHit hit, spacingDistance)) {
            steerAdjust += -1 + (carRightSide - hit.point).magnitude / spacingDistance;
        }

        if (Physics.Raycast(carLeftside, -transform.right, out hit, spacingDistance)) {
            steerAdjust += 1 + (carLeftside - hit.point).magnitude / spacingDistance;
        }

        return steerAdjust;
    }
}
