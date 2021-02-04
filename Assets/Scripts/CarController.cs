using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    // properties
    public float m_maxSpeed = 200.0f;
    public float m_maxTorque = 2500.0f;
    public float m_brakingTorque = 1875.0f;
    public float m_maxSteerAngle = 45.0f;
    public Vector3 m_centerOfMassOffset = new Vector3(0.0f, 0.3f, 0.6f);
    public float m_spoilerRatio = 0.15f;
    public float m_handbrakeForwardStiffness = 0.8f;
    public float m_handbrakeSidewayStiffness = 0.4f;
    public int m_numberOfGears = 5;
    public float m_revolutionsBoundary = 1.0f;

    protected float m_appliedBrakeTorque = 0.0f;
    protected bool m_handBraking = false;
    protected bool m_braking = false;
    protected bool m_reversing = false;
    protected int m_currentGear = 1;

    public float AccelerationInput { get; protected set; }
    public float CurrentSpeed { get; protected set; }
    public float TopSpeed { get { return m_maxSpeed; } }
    public int NumberOfGears { get { return m_numberOfGears; } }
    public int CurrentGear { get { return m_currentGear; } set { m_currentGear = value; } }
    public float Revolutions { get; set; }

    // containers
    public GameObject m_colliderContainer;
    public GameObject m_meshContainer;
    public GameObject m_brakeLightsContainer;

    // car parts
    protected WheelCollider[] m_wheelColliders;
    protected Rigidbody m_body;
    protected Transform[] m_wheelMeshes;
    protected Renderer[] m_brakeLightRenderers;

    protected Texture2D m_lightsIdleTex;
    protected Texture2D m_lightsBrakeTex;
    protected Texture2D m_lightsReverseTex;

    private void Awake()
    {
        GetWheelColliders();
        GetWheelMeshes();
        GetBrakeLightRenderers();
        LoadBrakeTextures();

        // offset center of mass for roll-over resistance
        m_body = GetComponent<Rigidbody>();
        m_body.centerOfMass += m_centerOfMassOffset;
    }

    protected void Update()
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
        UpdateBrakeLightState();
    }

    protected void SetStiffness(float foreSlip, float sideSlip)
    {
        // rear wheels only, start at index 2
        for (int i = 2; i < m_wheelColliders.Length; i++)
        {
            WheelCollider wc = m_wheelColliders[i];
            WheelFrictionCurve frictionCurve = wc.forwardFriction;
            frictionCurve.stiffness = foreSlip;
            wc.forwardFriction = frictionCurve;

            frictionCurve = wc.sidewaysFriction;
            frictionCurve.stiffness = sideSlip;
            wc.sidewaysFriction = frictionCurve;
        }
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
            if (m_wheelColliders[i].GetGroundHit(out contact))
            {
                Vector3 tempPos = m_wheelColliders[i].transform.position;
                // translate the position up from the point of contact, using collider radius for scaling
                tempPos.y = (contact.point + (m_wheelColliders[i].transform.up * m_wheelColliders[i].radius)).y;
                m_wheelMeshes[i].position = tempPos;
            }
        }
    }

    private void UpdateBrakeLightState()
    {
        if (m_braking || m_handBraking)
        {
            foreach (Renderer r in m_brakeLightRenderers)
            {
                r.material.mainTexture = m_lightsBrakeTex;
            }
        }
        else if (m_reversing)
        {
            foreach (Renderer r in m_brakeLightRenderers)
            {
                r.material.mainTexture = m_lightsReverseTex;
            }
        }
        else
        {
            foreach (Renderer r in m_brakeLightRenderers)
            {
                r.material.mainTexture = m_lightsIdleTex;
            }
        }
    }

    private void GetBrakeLightRenderers()
    {
        m_brakeLightRenderers = m_brakeLightsContainer.GetComponentsInChildren<Renderer>();
    }

    private void LoadBrakeTextures()
    {
        m_lightsIdleTex = Resources.Load<Texture2D>("Vehicle/LightsIdle");
        m_lightsBrakeTex = Resources.Load<Texture2D>("Vehicle/LightsBrake");
        m_lightsReverseTex = Resources.Load<Texture2D>("Vehicle/LightsReverse");
    }
}