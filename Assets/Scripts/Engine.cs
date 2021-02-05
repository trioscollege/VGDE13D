using UnityEngine;

[RequireComponent(typeof(CarController))]
public class Engine : MonoBehaviour
{ 
    public AudioClip m_accelClip;
    public float m_pitchMultiplier = 1f;
    public float m_lowPitchMin = 1f;
    public float m_lowPitchMax = 6f;
    public float m_highPitchMultiplier = 0.25f;
    public float m_maxRolloffDistance = 500;
    public float m_dopplerLevel = 1;
    public bool m_useDoppler = true;
    public bool m_is3DSound = true;

    public float m_revRangeBoundary = 1.0f;
    private float m_gearFactor;
    private AudioSource m_accel;
    private bool m_startedSound;
    private CarController m_carController;

    private AudioSource SetUpEngineAudioSource(AudioClip clip)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0;
        source.loop = true;

        source.time = Random.Range(0f, clip.length);
        source.Play();
        source.minDistance = 5;
        source.maxDistance = m_maxRolloffDistance;
        source.dopplerLevel = 0;
        source.spatialBlend = m_is3DSound ? 1.0f : 0.0f;
        return source;
    }

    private void StartSound()
    {
        m_carController = GetComponent<CarController>();
        m_accel = SetUpEngineAudioSource(m_accelClip);
        m_startedSound = true;
    }

    private void StopSound()
    {
        Destroy(m_accel);
        m_startedSound = false;
    }

    // simple function to add a curved bias towards 1 
    // for a value in the 0-1 range
    private static float CurveFactor(float factor)
    {
        return 1 - (1 - factor) * (1 - factor);
    }

    // unclamped version of Lerp 
    // to allow value to exceed the from-to range
    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }

    private void CalculateGearFactor()
    {
        float f = (1 / (float)m_carController.NumberOfGears);
        // current speed within the current gear's range of speeds.
        float targetGearFactor = Mathf.InverseLerp(f * m_carController.CurrentGear, f * (m_carController.CurrentGear + 1), Mathf.Abs(m_carController.CurrentSpeed / m_carController.TopSpeed));
        // smooth toward 'target' gear factor so revs don't snap when changing gear.
        m_gearFactor = Mathf.Lerp(m_gearFactor, targetGearFactor, Time.deltaTime * 5f);
    }

    private void CalculateRevs()
    {
        CalculateGearFactor();
        float gearNumFactor = m_carController.CurrentGear / (float)m_carController.NumberOfGears;
        float revsRangeMin = ULerp(0f, m_revRangeBoundary, CurveFactor(gearNumFactor));
        float revsRangeMax = ULerp(m_revRangeBoundary, 1f, gearNumFactor);
        m_carController.Revolutions = ULerp(revsRangeMin, revsRangeMax, m_gearFactor);
    }

    private void GearChanging()
    {
        float f = Mathf.Abs(m_carController.CurrentSpeed / m_carController.TopSpeed);
        float upgearlimit = (1 / (float)m_carController.NumberOfGears) * (m_carController.CurrentGear + 1);
        float downgearlimit = (1 / (float)m_carController.NumberOfGears) * m_carController.CurrentGear;

        if (m_carController.CurrentGear > 0 && f < downgearlimit)
        {
            m_carController.CurrentGear--;
        }

        if (f > upgearlimit && (m_carController.CurrentGear < (m_carController.NumberOfGears - 1)))
        {
            m_carController.CurrentGear++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // get the distance to main camera
        float camDist = (Camera.main.transform.position - transform.position).sqrMagnitude;

        // stop sound if the object is beyond the maximum roll off distance
        if (m_startedSound && camDist > m_maxRolloffDistance * m_maxRolloffDistance)
        {
            StopSound();
        }

        // start the sound if not playing and it is nearer than the maximum distance
        if (!m_startedSound && camDist < m_maxRolloffDistance * m_maxRolloffDistance)
        {
            StartSound();
        }

        if (m_startedSound)
        {
            CalculateRevs();
            GearChanging();

            // The pitch is interpolated between the min and max values, according to the car's revs.
            float pitch = ULerp(m_lowPitchMin, m_lowPitchMax, m_carController.Revolutions);

            // clamp to minimum pitch (note, not clamped to max for high revs while burning out)
            pitch = Mathf.Min(m_lowPitchMax, pitch);
           
            // for 1 channel engine sound, it's oh so simple:
            m_accel.pitch = pitch * m_pitchMultiplier * m_highPitchMultiplier;
            m_accel.dopplerLevel = m_useDoppler ? m_dopplerLevel : 0;
            m_accel.volume = 1;
        }
    }
}
