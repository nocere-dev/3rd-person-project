using System;
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


public enum EnemyState 
{
    Waiting,
    Moving, 
    Chasing,
    Searching
}

public class Enemy : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private EnemyMoveType moveType;
    [SerializeField] private EnemyClass enemyClass;
    [SerializeField] private Transform pathHolder;
    [SerializeField] private Transform player;
    public Transform eyePosition;

    [SerializeField] private LayerMask viewMask;
    
    
    [Header("View Settings")]
    [SerializeField] private Light spotLight;
    [SerializeField] private float viewDistance;
    private float viewAngle;
    

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float arriveDistance = 0.5f;


    private Vector3[] waypoints;
    private int targetIndex;
    private float waitTimer;

    private EnemyState enemyState = EnemyState.Moving;

    Color lightCol;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        lightCol = spotLight.color;
        viewAngle = spotLight.spotAngle;

        CacheWaypoints(); // Cache waypoints into an array on start 
        InitPatrol();
    }

    void Update()
    {
 
        if (CanSeePlayer())
        {
            spotLight.color = Color.red;
        }
        else
        {
            spotLight.color = lightCol;
        }
        
        if (moveType != EnemyMoveType.Patroling) return; // Checks enemy move type is patrolling type

        // If there are no waypoints/path holders created or waypoint count is less than 2 do nothing
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.Log("No or less than 2 waypoints created. Add more points.");
            return;
        } 

        if (enemyState == EnemyState.Moving)
        {
            PatrolMove();
        }
        else
        {
            PatrolWait();
        }

    }

    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if (angleToPlayer < viewAngle / 2f)
            {
                if (!Physics.Linecast(eyePosition.position, player.position, viewMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void InitPatrol()
    {
        // If waypoints are null do nothing
        if (waypoints == null || waypoints.Length == 0) return;

        transform.position = waypoints[0]; // Snap enemy to first waypoint

        targetIndex = (waypoints.Length > 1) ? 1 : 0;

        enemyState = EnemyState.Moving; // Set patrol state
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

        TurnToFace(target);

        // Move enemy towards next target that was set above
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Check if the enemy is close enough to the target waypoint to consider it as "arrived"
        // Squared distance instead of Vector3.Distance as it's cheaper
        if ((transform.position - target).sqrMagnitude <= arriveDistance * arriveDistance)
        {
            transform.position = target; // Snap enemy to target point to avoid floating point inaccuracy
            waitTimer = waitTime;
            enemyState = EnemyState.Waiting;
        }
    }

    private void TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLook = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - MathF.Atan2(dirToLook.z, dirToLook.x) * Mathf.Rad2Deg;

        float newAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
        
        transform.eulerAngles = Vector3.up * newAngle;
    }

    private void PatrolWait()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer > 0f) return;

        // Advance to next index and wrap index back to 0
        targetIndex = (targetIndex + 1) % waypoints.Length;
        enemyState = EnemyState.Moving;
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

        Gizmos.color = Color.red;
        Gizmos.DrawRay(eyePosition.position, transform.forward * viewDistance);
    }
}
