using UnityEngine;


[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(AudioSource))]
public class CarEngine : MonoBehaviour
{
    private CarController m_car;
    private AudioSource m_audioSource;

    // Start is called before the first frame update
    void Start()
    {
        m_car = GetComponent<CarController>();
        m_audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        float gearDifference;

    }
}
