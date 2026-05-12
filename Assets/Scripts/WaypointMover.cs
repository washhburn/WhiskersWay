using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float moveSpeed = 2f;
    public float waitTime = 2f;
    public bool loopWaypoints = true;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting;
    public bool isFrozen;

    public Vector2 CurrentMovement { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waypoints = new Transform[waypointParent.childCount];
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isFrozen)
        {
            CurrentMovement = Vector2.zero;
            return;
        }

        if (!isWaiting)
        {
            MoveToWaypoint();
        }
    }

    void MoveToWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        Vector2 direction = ((Vector2)targetWaypoint.position - (Vector2)transform.position).normalized;
        CurrentMovement = direction;

        transform.position = Vector2.MoveTowards(
            transform.position, 
            targetWaypoint.position, 
            moveSpeed * Time.deltaTime);
        
        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        CurrentMovement = Vector2.zero;
        yield return new WaitForSeconds(waitTime);

        currentWaypointIndex = loopWaypoints 
            ? (currentWaypointIndex + 1) % waypoints.Length 
            : Mathf.Min(currentWaypointIndex + 1, waypoints.Length - 1);

        isWaiting = false;
    }

}
