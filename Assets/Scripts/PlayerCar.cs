using UnityEngine;

public class PlayerCar : CarController
{
    void Update()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(m_body.velocity);
        // convert from m/s to km/h
        m_currentSpeed = localVelocity.z * 3.6f;
        // limit speed
        if (m_currentSpeed < m_maxSpeed)
        {
            // acceleration - front colliders only, indicies 0 and 1
            for (int i = 0; i < 2; i++)
            {
                m_wheelColliders[i].motorTorque = Input.GetAxis("Vertical") * m_maxTorque;
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
            m_wheelColliders[i].steerAngle = Input.GetAxis("Horizontal") * m_maxSteerAngle;
        }

        m_body.AddForce(-transform.up * (localVelocity.z * m_spoilerRatio), ForceMode.Force);

        // decelerating / braking
        bool torqueChange = false;
        if (((Input.GetAxis("Vertical") < 0 && localVelocity.z > 0 ||
            Input.GetAxis("Vertical") > 0 && localVelocity.z < 0)))
        {
            m_appliedBrakeTorque = m_brakingTorque + m_maxTorque;
            torqueChange = true;
        }
        // no acceleration input
        else if (Input.GetAxis("Vertical") == 0)
        {
            m_appliedBrakeTorque = m_brakingTorque;
            torqueChange = true;
        }
        else
        {
            m_appliedBrakeTorque = 0.0f;
            torqueChange = true;
        }

        // check for handbrake
        if (Input.GetButton("Handbrake"))
        {
            m_handBraking = true;
            m_appliedBrakeTorque = m_brakingTorque + m_maxTorque;
            torqueChange = true;
            if (m_body.velocity.sqrMagnitude > 0)
            {
                SetStiffness(
                    m_handbrakeForwardStiffness, 
                    m_handbrakeSidewayStiffness);
            }
            else
            {
                SetStiffness(1.0f, 1.0f);
            }
        }
        else
        {
            m_handBraking = false;
            SetStiffness(1.0f, 1.0f);
        }

        if (torqueChange)
        {
            for (int i = 0; i < m_wheelColliders.Length; i++)
            {
                m_wheelColliders[i].brakeTorque = m_appliedBrakeTorque;
            }
        }
    }
}