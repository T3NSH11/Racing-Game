using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointContainer : MonoBehaviour
{
    public List<Transform> waypointList;
    void Awake()
    {
        foreach(Transform transform in gameObject.GetComponentsInChildren<Transform>())
        {
            waypointList.Add(transform);
        }
        waypointList.Remove(waypointList[0]);
    }
}
