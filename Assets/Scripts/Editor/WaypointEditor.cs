using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaypointGizmos))]
public class WaypointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Flatten Waypoints"))
        {
            WaypointGizmos wg = target as WaypointGizmos;
            GameObject go = wg.gameObject;
            Transform[] waypoints = go.GetComponentsInChildren<Transform>();

            for (int i = 1; i < waypoints.Length; ++i)
            {
                Physics.Raycast(
                    new Ray(
                        waypoints[i].position + Vector3.up * 10000f,
                        Vector3.down * float.MaxValue),
                    out RaycastHit hit);

                waypoints[i].position = 
                    hit.point + Vector3.up * wg.m_flattenHeight;
            }
        }
    }
}