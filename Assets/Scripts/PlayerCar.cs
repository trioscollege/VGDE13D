using UnityEngine;

public class PlayerCar : CarController
{
    private new void Update()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(m_body.velocity);
        // convert from m/s to km/h
        CurrentSpeed = localVelocity.z * 3.6f;
        AccelerationInput = Input.GetAxis("Vertical");
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
            m_wheelColliders[i].steerAngle = Input.GetAxis("Horizontal") * m_maxSteerAngle;
        }

        m_body.AddForce(-transform.up * (localVelocity.z * m_spoilerRatio), ForceMode.Force);

        // decelerating / braking
        bool torqueChange = false;
        if ((AccelerationInput < 0 && localVelocity.z > 0) ||
            (AccelerationInput > 0 && localVelocity.z < 0))
        {
            m_appliedBrakeTorque = m_brakingTorque + m_maxTorque;
            torqueChange = true;
            m_braking = true;
        }
        // no acceleration input
        else if (AccelerationInput == 0)
        {
            m_appliedBrakeTorque = m_brakingTorque;
            torqueChange = true;
            m_braking = true;
        }
        else
        {
            m_appliedBrakeTorque = 0.0f;
            torqueChange = true;
            m_braking = false;
            m_reversing = localVelocity.z < 0;
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
        base.Update();
    }
}