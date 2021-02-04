using UnityEngine;

public class WaypointGizmos : MonoBehaviour
{
    public float m_radius = 1.0f;
    public Color m_color = Color.yellow;
    [Range(1, 10)]
    public int m_flattenHeight = 1;
    private Transform[] m_waypoints;

    void OnDrawGizmos()
    {
        m_waypoints = GetComponentsInChildren<Transform>();
        Vector3 lastWaypoint =
            m_waypoints[m_waypoints.Length - 1].position;

        for (int i = 1; i < m_waypoints.Length; i++)
        {
            Gizmos.color = m_color;
            Gizmos.DrawSphere(m_waypoints[i].position, m_radius);
            Gizmos.DrawLine(lastWaypoint, m_waypoints[i].position);
            lastWaypoint = m_waypoints[i].position;

            m_waypoints[i].name = "Waypoint " + i.ToString();
        }
    }
}