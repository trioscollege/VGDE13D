using UnityEngine;

public class PlayerCar : CarController
{
    void FixedUpdate()
    {
        // acceleration - front colliders only, indicies 0 and 1
        for (int i = 0; i < 2; i++)
        {
            m_wheelColliders[i].motorTorque = Input.GetAxis("Vertical") * m_maxTorque;
        }

        // steering - front colliders only, indicies 0 and 1
        for (int i = 0; i < 2; i++)
        {
            m_wheelColliders[i].steerAngle = Input.GetAxis("Horizontal") * m_maxSteerAngle;
        }

        Vector3 localVelocity = transform.InverseTransformDirection(m_body.velocity);
        m_body.AddForce(-transform.up * (localVelocity.z * m_spoilerRatio), ForceMode.Force);
    }
}