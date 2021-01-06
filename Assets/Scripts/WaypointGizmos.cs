using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointGizmos : MonoBehaviour
{
    public float m_size = 1.0f;
    public Color m_color = Color.yellow;
    private Transform[] m_waypoints;

    void OnDrawGizmos()
    {
        m_waypoints = GetComponentsInChildren<Transform>();
        Vector3 lastWaypoint = m_waypoints[m_waypoints.Length - 1].position;

        for (int i = 1; i < m_waypoints.Length; i++)
        {
            m_waypoints[i].name = "Waypoint " + i.ToString();
            Gizmos.color = m_color;
            Gizmos.DrawSphere(m_waypoints[i].position, m_size);
            Gizmos.DrawLine(lastWaypoint, m_waypoints[i].position);
            lastWaypoint = m_waypoints[i].position;
        }
    }
}
