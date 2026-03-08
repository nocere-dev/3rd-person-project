using System;
using UnityEngine;
using UnityEngine.UI;
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
    Alerted,
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
    
    [Space]
    [Header("View Settings")]
    [SerializeField] private Light spotLight;
    [SerializeField] private float viewDistance;
    [SerializeField] private float hearingRange;
    [SerializeField] private LayerMask decoyMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float killRange = 3;

    [Space]
    [Header("Detection Settings")]
    [SerializeField] private float detectionTime = 2f;
    [SerializeField] private float detectionDecayRate = 1f;
    [SerializeField] private Image detectionIcon;
    private float detectionMeter = 0f;
 
    [Space]
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float arriveDistance = 0.5f;
   
    private float viewAngle;
    private Color defaultLightColour;

    private Vector3[] waypoints;
    private int targetIndex;
    private float waitTimer;
    private Vector3 lastDecoyPos;
    private EnemyState enemyState = EnemyState.Moving;
    private bool distracted;
    
    private GameObject killTarget;

    void Start()
    {
        defaultLightColour = spotLight.color;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotLight.spotAngle;

        CacheWaypoints(); // Cache waypoints into an array on start 
        InitPatrol();
        distracted = false;
    }

    void Update()
    {
        CheckKillRange();
        
        if (CanSeePlayer())
        {
            detectionMeter += Time.deltaTime / detectionTime;
            detectionMeter = Mathf.Clamp01(detectionMeter);
            spotLight.color = Color.Lerp(defaultLightColour, Color.red, detectionMeter);

            if (detectionMeter >= 1f)
                enemyState = EnemyState.Alerted;
        }
        else
        {
            detectionMeter -= Time.deltaTime / detectionDecayRate;
            detectionMeter = Mathf.Clamp01(detectionMeter);
            spotLight.color = Color.Lerp(defaultLightColour, Color.red, detectionMeter);
        }

        UpdateDetectionUI();
        
        if (moveType != EnemyMoveType.Patroling) return; // Checks enemy move type is patrolling type

        // If there are no waypoints/path holders created or waypoint count is less than 2 do nothing
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.Log("Enemy patrol requires at least 2 waypoints.");
            return;
        }

        CheckHearingRange();

        switch (enemyState)
        {
            case EnemyState.Investigating:  InvestigateMove();  break;
            case EnemyState.Moving:         PatrolMove();       break;
            default:                        PatrolWait();       break;
        }
    }

    /// ------------------------------
    /// Detection
    /// ------------------------------
    bool CanSeePlayer()
    {
        
        if (Vector3.Distance(transform.position, player.position) >= viewDistance)
            return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer >= viewAngle / 2f)
            return false;
        
        return !Physics.Linecast(eyePosition.position, player.position, viewMask);
    }

    //detects colliders entering the hearing range
    private void CheckHearingRange()
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

    private void UpdateDetectionUI()
    {
        detectionIcon.fillAmount = detectionMeter;
        detectionIcon.gameObject.SetActive(detectionMeter > 0f);
    }

    /// ------------------------------
    /// Combat
    /// ------------------------------
    private void CheckKillRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, killRange, playerMask);
        killTarget = colliders.Length > 0 ? colliders[0].gameObject : null;
    }

    private void TryKillPlayer()
    {
        if (killTarget == null) return;

        Destroy(killTarget);
        SceneManager.LoadScene("Scenes/You Died");
    }

    /// ------------------------------
    /// Patrol
    /// ------------------------------

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
            waypoints[i] = pathHolder.GetChild(i).position;
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

    private void PatrolWait()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer > 0f) return;

        // Advance to next index and wrap index back to 0
        targetIndex = (targetIndex + 1) % waypoints.Length;
        enemyState = EnemyState.Moving;
    }
    
    //move towards whatever has caught their attention
    private void InvestigateMove()
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

    /// ------------------------------
    /// Utility
    /// ------------------------------

    private void TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLook = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - MathF.Atan2(dirToLook.z, dirToLook.x) * Mathf.Rad2Deg;

        float newAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
        
        transform.eulerAngles = Vector3.up * newAngle;
    }

    /// ------------------------------
    /// Gizmos
    /// ------------------------------

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
