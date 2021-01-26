﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    // properties
    public float m_maxSpeed = 200.0f;
    protected float m_currentSpeed = 0.0f;
    public float m_maxTorque = 2500.0f;
    public float m_brakingTorque = 1875.0f;
    protected float m_appliedBrakeTorque = 0.0f;
    public float m_maxSteerAngle = 45.0f;
    public Vector3 m_centerOfMassOffset = new Vector3(0.0f, 0.3f, 0.6f);
    public float m_spoilerRatio = 0.15f;

    // containers
    public GameObject m_colliderContainer;
    public GameObject m_meshContainer;

    // car parts
    protected WheelCollider[] m_wheelColliders;
    protected Rigidbody m_body;
    protected Transform[] m_wheelMeshes;

    private void Awake()
    {
        GetWheelColliders();
        GetWheelMeshes();

        // offset center of mass for roll-over resistance
        m_body = GetComponent<Rigidbody>();
        m_body.centerOfMass += m_centerOfMassOffset;
    }

    private void Update()
    {
        for (int i = 0; i < m_wheelMeshes.Length; i++)
        {
            float rotationThisFrame = m_wheelColliders[i].rpm * Time.deltaTime * 6.0f;
            m_wheelMeshes[i].Rotate(rotationThisFrame, 0, 0);
        }

        // front tires only, indicies 0 and 1
        for (int i = 0; i < 2; i++)
        {
            m_wheelMeshes[i].localEulerAngles = new Vector3(m_wheelMeshes[i].localEulerAngles.x,
                                                            m_wheelColliders[i].steerAngle - m_wheelMeshes[i].localEulerAngles.z,
                                                            m_wheelMeshes[i].localEulerAngles.z);
        }

        UpdateWheelPositions();
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

    private void GetWheelMeshes()
    {
        m_wheelMeshes = new Transform[4];
        Transform[] m_children = m_meshContainer.GetComponentsInChildren<Transform>();
        for (int i = 1; i < m_children.Length; i++) // i = 1 to ignore parent transform
        {
            m_wheelMeshes[i - 1] = m_children[i];
        }
    }

    private void UpdateWheelPositions()
    {
        WheelHit contact = new WheelHit();

        for (int i = 0; i < m_wheelMeshes.Length; i++)
        {
            // check for contact with "ground"
            if (m_wheelColliders[i].GetGroundHit(out contact))
            {
                // sample the current position of the collider
                Vector3 tempPos = m_wheelColliders[i].transform.position;
                // translate the position up from the point of contact, using collider radius for scaling
                tempPos.y = (contact.point + (m_wheelColliders[i].transform.up * m_wheelColliders[i].radius)).y;
                // place mesh "axel:" at new position
                m_wheelMeshes[i].position = tempPos;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.TransformPoint(m_centerOfMassOffset), 0.25f);
    }
}