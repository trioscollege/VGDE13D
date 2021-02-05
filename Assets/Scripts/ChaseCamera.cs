using UnityEngine;

public class ChaseCamera : MonoBehaviour
{
    public Transform m_chaseTarget;
    public float m_distance = 7.0f;
    public float m_height = 3.0f;
    public float m_rotationDamping = 3.0f;
    public float m_heightDamping = 2.0f;
    private float m_desiredAngle = 0.0f;
    private float m_desiredHeight = 0.0f;
    private Rigidbody m_rigidbody;

    void Start()
    {
        m_rigidbody = m_chaseTarget.GetComponent<Rigidbody>(); 
    }

    void LateUpdate()
    {
        // Get the current yaw and height of the camera
        float currentAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // Figure out desired rotation and height for the camera
        m_desiredAngle = m_chaseTarget.eulerAngles.y;
        m_desiredHeight = m_chaseTarget.position.y + m_height;

        // Verify m_chaseTarget has a rigidbody
        if (m_rigidbody)
        {
            Vector3 localVelocity =
                m_chaseTarget.InverseTransformDirection(
                    m_rigidbody.velocity);
            // Verify local z velocity is less than -0.5f
            if (localVelocity.z < -0.5f)
            {
                m_desiredAngle += 180.0f;
            }
        }

        // Figure out adjustments to move from current values toward desired values over time
        currentAngle = Mathf.LerpAngle(currentAngle, m_desiredAngle, m_rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, m_desiredHeight, m_heightDamping * Time.deltaTime);

        // Create a quaternion from new yaw angle
        Quaternion currentRotation = Quaternion.Euler(0, currentAngle, 0);
        // Create final position for camera by rotating a forward vector behind the target and scaling to the chase distance
        Vector3 finalPosition = m_chaseTarget.position - (currentRotation * Vector3.forward * m_distance);

        // Set the final height to the new height
        finalPosition.y = currentHeight;
        // Move the camera
        transform.position = finalPosition;
        // Make sure the camera is looking at the chase target
        transform.LookAt(m_chaseTarget);
    }
}