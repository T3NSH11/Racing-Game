using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CarMechanics))]
public class AICarController : MonoBehaviour
{
    [SerializeField]
    private WaypointContainer waypointContainer;
    private List<Transform> waypoints;
    private int currentWaypoint;
    private CarMechanics playerController;
    [SerializeField]
    private float waypointRange;
    private float currAngle;
    private float accelInput;
    [SerializeField]
    private float accelDampen;
    [SerializeField]
    private float maxAngle = 45f;
    [SerializeField]
    private float maxSpeed = 120f;
    [Range(0.01f, 0.04f)]
    [SerializeField]
    private float steeringConstant = 0.02f;

    public bool isInsideBraking;

    private void Start()
    {
        InitializeController();
    }

    private void Update()
    {
        HandleWaypointNavigation();
        HandleCarControl();
    }

    private void InitializeController()
    {
        playerController = GetComponent<CarMechanics>();
        waypoints = waypointContainer.waypointList;
        currentWaypoint = 0;
    }

    private void HandleWaypointNavigation()
    {
        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) < waypointRange)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Count) 
                currentWaypoint = 0;
        }
    }

    private void HandleCarControl()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        currAngle = Vector3.SignedAngle(fwd, waypoints[currentWaypoint].position - transform.position, Vector3.up);
        accelInput = Mathf.Clamp01((1f - Mathf.Abs(playerController.speed * 0.02f * currAngle) / maxAngle));
        if (isInsideBraking)
        {
            accelInput = -accelInput * ((Mathf.Clamp01((playerController.speed) / maxSpeed) * 2 - 1f));
        }
        accelDampen = Mathf.Lerp(accelDampen, accelInput, Time.deltaTime * 3f);
        playerController.SetInput(accelDampen, currAngle, 0, 0);
        Debug.DrawRay(transform.position, waypoints[currentWaypoint].position - transform.position, Color.yellow);
    }
}
