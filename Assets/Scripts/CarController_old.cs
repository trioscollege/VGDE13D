using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController_old : MonoBehaviour
{
    public float m_maxTorque = 100.0f;
    public float m_brakingTorque = 75.0f;
    public float m_maxSteerAngle = 25.0f;
    public float m_maxSpeed = 200.0f;
    public int m_numberOfGears = 5;
    [Tooltip("Normalized percentage of forward velocity as downward force.")]
    public float m_spoilerRatio = 0.15f;
    public Vector3 m_centerOfMassOffset = new Vector3(0.0f, 0.2f, 0.4f);

    public GameObject m_wheelContainer;
    public GameObject m_colliderContainer;
    public GameObject m_brakeLightsContainer;
    public float m_handbrakeFowardSlip = 0.08f;
    public float m_handbrakeSidewaysSlip = 0.04f;

    public bool Finished { get; set; }

    protected Rigidbody m_body;
    protected Transform[] m_wheelMeshes;
    protected WheelCollider[] m_wheelColliders;
    protected Renderer[] m_brakeLightRenderers;

    protected float m_currentSpeed;
    
    protected bool m_handBraking = false;
    protected bool m_braking = false;
    protected bool m_reversing = false;

    protected Texture2D m_lightsIdleTex;
    protected Texture2D m_lightsBrakeTex;
    protected Texture2D m_lightsReverseTex;

    private float m_gearSpread;

    void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        m_body.centerOfMass += m_centerOfMassOffset;

        GetWheelMeshes();
        GetWheelColliders();
        GetBrakeLightRenderers();
        LoadBrakeTextures();

        m_gearSpread = m_maxSpeed / m_numberOfGears;
    }

    // Update is called once per frame
    void Update()
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
        UpdateEngineSound();
    }

    protected void SetSlipValues(float forwardSlip, float sidewaysSlip)
    {
        //foreach (WheelCollider wc in m_wheelColliders)
        for (int i = 2; i < m_wheelColliders.Length; i++)
        {
            WheelCollider wc = m_wheelColliders[i];
            WheelFrictionCurve frictionCurve = wc.forwardFriction;
            frictionCurve.stiffness = forwardSlip;
            wc.forwardFriction = frictionCurve;
            
            frictionCurve = wc.sidewaysFriction;
            frictionCurve.stiffness = sidewaysSlip;
            wc.sidewaysFriction = frictionCurve;
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

    private void UpdateEngineSound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        if (audioSource)
        {
            if (m_currentSpeed > 0)
            {
                if (m_currentSpeed > m_maxSpeed)
                {
                    audioSource.pitch = 1.75f;
                }
                else
                {
                    audioSource.pitch = ((m_currentSpeed % m_gearSpread) / m_gearSpread) + 0.75f;
                }
            }
        }
    }

    private void GetWheelMeshes()
    {
        m_wheelMeshes = new Transform[4];
        Transform[] m_children = m_wheelContainer.GetComponentsInChildren<Transform>();
        for (int i = 1; i < m_children.Length; i++) // i = 1 to ignore parent transform
        {
            m_wheelMeshes[i - 1] = m_children[i];
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

    private void GetBrakeLightRenderers()
    {
        m_brakeLightRenderers = m_brakeLightsContainer.GetComponentsInChildren<Renderer>();
    }

    private void LoadBrakeTextures()
    {
        m_lightsIdleTex = Resources.Load("Vehicle/LightsIdle") as Texture2D;
        m_lightsBrakeTex = Resources.Load("Vehicle/LightsBrake") as Texture2D;
        m_lightsReverseTex = Resources.Load("Vehicle/LightsReverse") as Texture2D;
    }
}
