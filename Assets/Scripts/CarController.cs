using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    // properties
    public float m_maxTorque = 100.0f;
    public float m_maxSteerAngle = 25.0f;

    // containers
    public GameObject m_colliderContainer;

    // car parts
    protected WheelCollider[] m_wheelColliders;

    private void Awake()
    {
        GetWheelColliders();
    }

    private void GetWheelColliders()
    {
        m_wheelColliders = new WheelCollider[4];
        WheelCollider[] m_children = m_colliderContainer.GetComponentsInChildren<WheelCollider>();
        for (int i = 0; i < m_children.Length; i++)
        {
            m_wheelColliders[i] = m_children[i];
        }
    }
}