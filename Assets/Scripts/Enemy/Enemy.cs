using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using UnityEngine.AI;
using System.Threading;

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
    Investigating,
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
    [SerializeField] private GameObject lastKnownMarkerPrefab;
    private GameObject currentMarker;
    private float detectionMeter = 0f;
 
    [Space]
    [Header("Movement Settings")]
    private NavMeshAgent agent;
    [SerializeField] private float waitTime = 1f;
   
    [Space]
    [Header("Search Settings")]
    [SerializeField] private float searchTime = 2f;
    private float viewAngle;
    private Color defaultLightColour;

    private Vector3[] waypoints;
    private int targetIndex;

    private float waitTimer;
    private float searchTimer;

    private Vector3 lastDecoyPos;
    private EnemyState enemyState = EnemyState.Moving;
    private Vector3 lastKnownPlayerPos;
    private bool distracted;
    
    private GameObject killTarget;

    private ParticleSystem decoyParticles;
    private float distractedTimer;
    private bool reachedDecoy = false;
    private bool reachedSearchPoint = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
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

            if (detectionMeter >= 1f && enemyState != EnemyState.Chasing)
            {
                enemyState = EnemyState.Chasing;
                TryKillPlayer();
            }
        }
        else
        {
            detectionMeter -= Time.deltaTime / detectionDecayRate;
            detectionMeter = Mathf.Clamp01(detectionMeter);
            spotLight.color = Color.Lerp(defaultLightColour, Color.red, detectionMeter);

            if (enemyState == EnemyState.Chasing)
            {
                lastKnownPlayerPos = player.position;
                searchTimer = searchTime;
                enemyState = EnemyState.Searching;

                if (currentMarker != null) Destroy(currentMarker);
                currentMarker = Instantiate(lastKnownMarkerPrefab, lastKnownPlayerPos, quaternion.identity);
            }
        }

        UpdateDetectionUI();
        
        switch (enemyState)
        {
            case EnemyState.Chasing:        ChasePlayer();      return;
            case EnemyState.Searching:      SearchMove();       return;
            case EnemyState.Investigating:  InvestigateMove();  return;
        }
        if (moveType != EnemyMoveType.Patroling) return; // Checks enemy move type is patrolling type

        // If there are no waypoints/path holders created or waypoint count is less than 2 do nothing
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.Log("Enemy patrol requires at least 2 waypoints.");
            return;
        }

        switch (enemyState)
        {
            case EnemyState.Moving:         PatrolMove();       break;
            default:                        PatrolWait();       break;
        }
 
        if (!distracted)
            CheckHearingRange();
   }

    /// ------------------------------
    /// Detection
    /// ------------------------------
    bool CanSeePlayer()
    {
        if (player.GetComponent<Player>().isHidden) return false;
        
        if (Vector3.Distance(transform.position, player.position) >= viewDistance)
            return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer >= viewAngle / 2f)
            return false;
        
        return !Physics.Raycast(eyePosition.position, dirToPlayer, viewDistance, viewMask, QueryTriggerInteraction.Collide);
    }

    //detects colliders entering the hearing range
    private void CheckHearingRange()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, hearingRange, decoyMask);

        if (colliders.Length > 0 && enemyState != EnemyState.Chasing)
        {
            distracted = true;
            lastDecoyPos = colliders[0].transform.position;
            
            decoyParticles = colliders[0].GetComponent<ParticleSystem>();
            distractedTimer = decoyParticles != null ? decoyParticles.main.duration : 3f;
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
    private void ChasePlayer()
    {
       lastKnownPlayerPos = player.position;
       agent.SetDestination(player.position);
    }

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

        agent.Warp(waypoints[0]);

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
        agent.SetDestination(waypoints[targetIndex]);

        if (HasArrived())
        {
            waitTimer = waitTime;
            enemyState = EnemyState.Waiting;
        }
    }

    private bool HasArrived()
    {
        return !agent.pathPending 
        && agent.remainingDistance <= agent.stoppingDistance
        && agent.velocity.sqrMagnitude == 0f;
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
        agent.SetDestination(lastDecoyPos);

        if (HasArrived() && !reachedDecoy)
        {
            reachedDecoy = true;
            agent.ResetPath();
        }
        if (reachedDecoy)
        {
            transform.eulerAngles += Vector3.up * 60f * Time.deltaTime;
            distractedTimer -= Time.deltaTime;
            if (distractedTimer <= 0f)
            {
                distracted = false;
                reachedDecoy = false;
                enemyState = EnemyState.Moving;
            }
            
        }
   }

    private void SearchMove()
    {
        agent.SetDestination(lastKnownPlayerPos);

        if (HasArrived() && !reachedSearchPoint)
        {
            reachedSearchPoint = true;
            agent.ResetPath();
        }

        if (reachedSearchPoint)
        {
            transform.eulerAngles += Vector3.up * 60f * Time.deltaTime;
            searchTimer -= Time.deltaTime;

            if (searchTimer <= 0f)
            {
                detectionMeter = 0f;
                reachedSearchPoint = false;
                enemyState = EnemyState.Moving;
                if (currentMarker != null) Destroy(currentMarker);
            }
        }
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
