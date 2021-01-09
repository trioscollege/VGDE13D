using UnityEngine;

public class PlayerCar_old : CarController_old
{
    void FixedUpdate()
    {
        // sample rear left tire speed, index 2 - calculate KM/H from RPM (assume a radius 10x bigger)
        m_currentSpeed = ((20 * Mathf.PI) / 60.0f) * m_wheelColliders[2].radius * m_wheelColliders[2].rpm;
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
            for (int i = 0; i < 2; i++)
            {
                m_wheelColliders[i].motorTorque = 0;
            }
        }

        Vector3 localVelocity = transform.InverseTransformDirection(m_body.velocity);
        m_body.AddForce(-transform.up * (localVelocity.z * m_spoilerRatio), ForceMode.Force);

        // front colliders only, indicies 0 and 1
        for (int i = 0; i < 2; i++)
        {
            m_wheelColliders[i].steerAngle = Input.GetAxis("Horizontal") * m_maxSteerAngle;
        }

        // decelerating / braking
        if (Input.GetAxis("Vertical") < 0 && localVelocity.z > 0 ||
            Input.GetAxis("Vertical") > 0 && localVelocity.z < 0)
        {
            for (int i = 0; i < m_wheelColliders.Length; i++)
            {
                m_wheelColliders[i].brakeTorque = m_brakingTorque + m_maxTorque;
            }

            m_braking = true;
            m_reversing = false;
        }
        // no acceleration input
        else if (Input.GetAxis("Vertical") == 0)
        {
            for (int i = 0; i < m_wheelColliders.Length; i++)
            {
                m_wheelColliders[i].brakeTorque = m_brakingTorque;
            }
            m_braking = false;
            m_reversing = false;
        }
        // accelerating / reversing
        else
        {
            for (int i = 0; i < m_wheelColliders.Length; i++)
            {
                m_wheelColliders[i].brakeTorque = 0;
            }
            m_braking = false;
            m_reversing = localVelocity.z < 0;
        }
        
        // apply handbrake
        if (Input.GetKey(KeyCode.Space))
        {
            m_handBraking = true;
            foreach (WheelCollider wc in m_wheelColliders)
            {
                wc.brakeTorque = m_brakingTorque + m_maxTorque;
            }
            if (m_body.velocity.sqrMagnitude > 0)
            {
                
                SetSlipValues(m_handbrakeFowardSlip, m_handbrakeSidewaysSlip);
            }
            else
            {
                SetSlipValues(1.0f, 1.0f);
            }
        }
        else
        {
            m_handBraking = false;
            foreach(WheelCollider wc in m_wheelColliders)
            {
                wc.brakeTorque = 0;
            }
            SetSlipValues(1.0f, 1.0f);
        }
    }
}
