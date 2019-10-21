// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class RandomFly : MonoBehaviour 
{
    public float speed = 5;
    public float maxHeadingChange = 30;

    private float heading;
    private Vector3 targetRotation;

    public AnimationClip flyAnimation;
    public float flyAnimationSpeed = 1;


    void Start() {
        heading = Random.Range(0, 360);
        transform.eulerAngles = new Vector3(0, heading, 0);
        StartCoroutine(NewHeading());
	}
    
    void Update() {
        transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, targetRotation, Time.deltaTime * 2);
        transform.Translate(new Vector3(0,0,speed) * Time.deltaTime, Space.Self);
        GetComponent<Animation>()[flyAnimation.name].speed = flyAnimationSpeed;
        GetComponent<Animation>().CrossFade(flyAnimation.name, 0.2f);
	}

    IEnumerator NewHeading() {
        while(true) {
            NewHeadingRoutine();
            yield return new WaitForSeconds(3);
		}
    }

    void NewHeadingRoutine() {
        float floor = Mathf.Clamp(heading - maxHeadingChange, 0, 360);
        float ceil = Mathf.Clamp(heading + maxHeadingChange, 0, 360);
        heading = Random.Range(floor, ceil);
        targetRotation = new Vector3(0, heading, 0);
    }
}