using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class SkidEnabler : MonoBehaviour
{

    public float m_stiffnessThreshold = 0.1f;
    public float m_decayTime = 4.0f;
    [Tooltip("The amount, in units, to raise skidmark from the base of the wheel collider.")]
    public float m_placementOffset = 0.01f;
    private WheelCollider m_wheelCollider;
    private GameObject m_skidObject;
    private TrailRenderer m_renderer;

    void Awake()
    {
        m_wheelCollider = GetComponent<WheelCollider>();
    }

    void LateUpdate()
    {
        WheelHit hit;
        if (m_wheelCollider.forwardFriction.stiffness <= m_stiffnessThreshold && m_wheelCollider.GetGroundHit(out hit))
        {
            if (m_skidObject == null)
            {
                m_skidObject = PoolManager.Instance.GetObjectOfType("SkidMark", false);
                m_skidObject.transform.position = hit.point + (m_placementOffset * Vector3.up);
                m_skidObject.transform.parent = m_wheelCollider.transform;
                m_renderer = m_skidObject.GetComponent<TrailRenderer>();
                m_renderer.Clear();
            }

            m_skidObject.transform.localPosition = transform.InverseTransformPoint(hit.point) + (m_placementOffset * Vector3.up);
        }
        else
        {
            if (m_renderer)
            {
                PoolManager.Instance.PoolObject(m_skidObject, true);
                m_skidObject = null;
                m_renderer = null;
            }
        }
    }
}
