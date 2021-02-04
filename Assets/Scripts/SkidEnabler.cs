using UnityEngine;

[RequireComponent(typeof(WheelCollider)),
 RequireComponent(typeof(AudioSource))]
public class SkidEnabler : MonoBehaviour
{
    public float m_slipLimit = 0.9f;
    public float m_placementOffset = 0.01f;
    private WheelCollider m_wheelCollider;
    private GameObject m_skidObject;
    private TrailRenderer m_renderer;
    private AudioSource m_audioSource;

    void Awake()
    {
        m_wheelCollider = GetComponent<WheelCollider>();
        m_audioSource = GetComponent<AudioSource>();
    }

    void LateUpdate()
    {
        m_wheelCollider.GetGroundHit(out WheelHit hit);

        // verify that the wheel is "skidding"
        if (Mathf.Abs(hit.forwardSlip) > m_slipLimit ||
            Mathf.Abs(hit.sidewaysSlip) > m_slipLimit)
        {
            // check for existing object
            if (m_skidObject == null)
            {
                // retrieve object from the pool (only pooled objects)
                m_skidObject = PoolManager.Instance.GetObjectOfType("SkidMark", false);
                // move it into position
                m_skidObject.transform.position = hit.point + (m_placementOffset * Vector3.up);
                // attach it to the collider
                m_skidObject.transform.parent = m_wheelCollider.transform;
                // retrieve a reference to the trail renderer
                m_renderer = m_skidObject.GetComponent<TrailRenderer>();
                // clear existing trails.
                m_renderer.Clear();

                m_audioSource.Play();
            }

            // continually update skid mark position to the point of contact
            m_skidObject.transform.localPosition =
                transform.InverseTransformPoint(hit.point) + (m_placementOffset * Vector3.up);
        }
        else
        {
            // verify the object hasn't been returned
            if (m_skidObject)
            {
                // pool object
                PoolManager.Instance.PoolObject(m_skidObject, true);
                m_skidObject = null;
                m_renderer = null;

                m_audioSource.Stop();
            }
        }
    }
}
