using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AntiRollBar : MonoBehaviour
{
    public WheelCollider m_leftWheel;
    public WheelCollider m_rightWheel;
    public float m_maxAntiRollForce = 5000.0f;
    private Rigidbody m_rigidbody;

    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        WheelHit hit;
        float leftTravel = 1.0f;
        float rightTravel = 1.0f;

        bool isLeftGrounded = m_leftWheel.GetGroundHit(out hit);
        if (isLeftGrounded)        
        {
            leftTravel = (-m_leftWheel.transform.InverseTransformPoint(hit.point).y - m_leftWheel.radius) / m_leftWheel.suspensionDistance;
        }

        bool isRightGrounded = m_rightWheel.GetGroundHit(out hit);
        if (isRightGrounded)        
        {
            rightTravel = (-m_rightWheel.transform.InverseTransformPoint(hit.point).y - m_rightWheel.radius) / m_rightWheel.suspensionDistance;
        }

        float antiRollForce = (leftTravel - rightTravel) * m_maxAntiRollForce;

        if (isLeftGrounded)
        {
            m_rigidbody.AddForceAtPosition(m_leftWheel.transform.up * -antiRollForce, m_leftWheel.transform.position);
        }

        if (isRightGrounded)
        {
            m_rigidbody.AddForceAtPosition(m_rightWheel.transform.up * antiRollForce, m_rightWheel.transform.position);
        }
    }
}
