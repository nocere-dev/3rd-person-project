using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum EnemyMoveType
{
    Static,
    Patroling
}

public enum EnemyClass
{
    Light,
    Medium,
    Heavy
}



public class Enemy : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private EnemyMoveType moveType;
    [SerializeField] private EnemyClass enemyClass;

    [SerializeField] private Transform pathHolder;

    [SerializeField] private float viewDistance = 15f;
    [SerializeField] private float fov = 45f;
    public Transform eyePosition;
    

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float arriveDistance = 0.5f;

    private Vector3[] waypoints;
    private int targetIndex;
    private float waitTimer;

    private enum PatrolState { Moving, Waiting }
    private PatrolState patrolState = PatrolState.Moving;

    void Start()
    {
        CacheWaypoints(); // Cache waypoints into an array on start 
        InitPatrol();
    }

    void Update()
    {
        if (moveType != EnemyMoveType.Patroling) return; // Checks enemy move type is patrolling type

        // If there are no waypoints/path holders created or waypoint count is less than 2 do nothing
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.Log("No or less than 2 waypoints created. Add more points.");
            return;
        } 

        if (patrolState == PatrolState.Moving)
        {
            PatrolMove();
        }
        else
        {
            PatrolWait();
        }
    }

    private void InitPatrol()
    {
        // If waypoints are null do nothing
        if (waypoints == null || waypoints.Length == 0) return;

        transform.position = waypoints[0]; // Snap enemy to first waypoint

        targetIndex = (waypoints.Length > 1) ? 1 : 0;

        patrolState = PatrolState.Moving; // Set patrol state
        waitTimer = 0f; // Guarantee no leftover state
    }

    private void CacheWaypoints()
    {
        // If there's no pathHolder set waypoints to null for other checks and do nothing
        if (!pathHolder)
        {
            Debug.Log("pathHolder not found, create a pathHolder.");
            waypoints = null;
            return;
        }

        // Order waypoints from the order in the unity heirarchy
        int count = pathHolder.childCount;
        waypoints = new Vector3[count]; // Allocating the waypoint array

        // Loop through each child transform of pathHolder
        for (int i = 0; i < count; i++)
        {
            // Read and store waypoints world position and store in the array
            waypoints[i] = pathHolder.GetChild(i).position;
        }
    }

    private void PatrolMove()
    {
        // Set current waypoint to the enemies next target position
        Vector3 target = waypoints[targetIndex];

        // Move enemy towards next target that was set above
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Check if the enemy is close enough to the target waypoint to consider it as "arrived"
        // Squared distance instead of Vector3.Distance as it's cheaper
        if ((transform.position - target).sqrMagnitude <= arriveDistance * arriveDistance)
        {
            transform.position = target; // Snap enemy to target point to avoid floating point inaccuracy
            waitTimer = waitTime;
            patrolState = PatrolState.Waiting;
        }
    }

    private void PatrolWait()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer > 0f) return;

        // Advance to next index and wrap index back to 0
        targetIndex = (targetIndex + 1) % waypoints.Length;
        patrolState = PatrolState.Moving;
    }

    void OnDrawGizmos()
    {
        Vector3 startPos = pathHolder.GetChild (0).position;
        Vector3 prevPos = startPos;

        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(prevPos, waypoint.position);
            prevPos = waypoint.position;
        }

        Gizmos.DrawLine(prevPos, startPos);
    }
}
