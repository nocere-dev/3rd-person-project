using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    Searching,
    Investigating
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
    
    [SerializeField] private float hearingRange;
    private Vector3 lastDecoyPos;
    private bool distracted;
    [SerializeField] private LayerMask decoyMask;
    
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float killRange = 3;
    private GameObject target;
    public bool canKill;

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

    void Start()
    {
        lightCol = spotLight.color;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotLight.spotAngle;

        CacheWaypoints(); // Cache waypoints into an array on start 
        InitPatrol();
        distracted = false;
    }

    void Update()
    {
 
        KillPlayer();
        
        if (CanSeePlayer())
        {
            spotLight.color = Color.red;
            exterminate();
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

        earRadius();

        if (enemyState == EnemyState.Investigating)
        {
            investigateMove();
        }
        else if (enemyState == EnemyState.Moving)
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

    
    //detects colliders entering the hearing range
    private void earRadius()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, hearingRange, decoyMask);

        if (colliders.Length > 0)
        {
            distracted = true;
            lastDecoyPos = colliders[0].transform.position;
            enemyState = EnemyState.Investigating;
            Debug.Log("HUUUUUUUUUUUUUUUUUUUUUHHHHHHHHH");
        }
    }


    //move towards whatever has caught their attention
    private void investigateMove()
    {
        Vector3 targetPos = new Vector3(lastDecoyPos.x, transform.position.y, lastDecoyPos.z);
        
        TurnToFace(targetPos);

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if((transform.position - targetPos).sqrMagnitude <= arriveDistance * arriveDistance)
        {
            distracted = false;
            enemyState = EnemyState.Moving;
        }
    }

    //checks to see if it can kill the player
    private void KillPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, killRange, playerMask);

        if(colliders.Length > 0)
        {
            canKill = true;
            target = colliders[0].gameObject;
            SceneManager.LoadScene("Scenes/You Died");
        }
        else
        {
            canKill = false;
            target = null;
        }
        
    }

    private void exterminate()
    {
        if(canKill && target != null)
        {
            Destroy(target);

            canKill = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, killRange);
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
