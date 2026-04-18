using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace Enemy {

public enum EnemyMoveType {
    Static,
    Patroling
}

public enum EnemyState {
    Waiting,
    Moving,
    Chasing,
    Investigating,
    Searching
}

[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "Enemy")]
public class Enemy : MonoBehaviour {
    [Header("AI Settings")] [SerializeField]
    private EnemyMoveType moveType;

    [SerializeField] private Animator animator;
    [SerializeField] private Transform pathHolder;
    [SerializeField] private Transform player;
    public Transform eyePosition;
    [SerializeField] private LayerMask viewMask;

    [Space] [Header("View Settings")] [SerializeField]
    private Light spotLight;

    [SerializeField] private float viewDistance;
    [SerializeField] private float _maxVerticalAngle = 30f;
    [SerializeField] private float hearingRange;
    [SerializeField] private LayerMask decoyMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float killRange = 3;
    [SerializeField] private bool showViewGizmos = true;
    [SerializeField] [Range(4, 40)] private int viewGizmoRays = 12;

    [Space] [Header("Detection Settings")] [SerializeField]
    private float detectionTime = 2f;

    [SerializeField] private float detectionDecayRate = 1f;
    [SerializeField] private Image detectionIcon;
    [SerializeField] private GameObject lastKnownMarkerPrefab;
    [SerializeField] private float waitTime = 1f;

    [Space]
    [Header("Search Settings")]
    [SerializeField] private float searchTime = 2f;

    [Space]
    [Header("Movement Settings")]
    private NavMeshAgent _agent;

    private Player _playerComponent;

    private GameObject _currentMarker;

    private ParticleSystem _decoyParticles;
    private Color _defaultLightColour;
    private float _detectionMeter;
    private bool _distracted;
    private float _distractedTimer;
    private EnemyState _enemyState = EnemyState.Moving;

    private GameObject _killTarget;

    private Vector3 _lastDecoyPos;
    private Vector3 _lastKnownPlayerPos;
    private bool _reachedDecoy;
    private bool _reachedSearchPoint;
    private float _searchTimer;
    private int _targetIndex;
    private float _viewAngle;

    private float _waitTimer;

    private Vector3[] _waypoints;
    private readonly Collider[] _hearingHits = new Collider[8];
    private readonly Collider[] _killHits = new Collider[2];

    private static readonly Color ViewGizmoColor = new Color(0.1f, 0.8f, 1f, 1f);

    private void Start() {
        _agent = GetComponent<NavMeshAgent>();
            if (animator == null)
                animator = GetComponent<Animator>();
            if (!animator) animator = GetComponentInChildren<Animator>();
            _defaultLightColour = spotLight.color;
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) {
            player = playerObject.transform;
            _playerComponent = playerObject.GetComponent<Player>();
        }

        _viewAngle = spotLight.spotAngle;

        CacheWaypoints(); // Cache waypoints into an array on start 
        InitPatrol();
        _distracted = false;
    }

    private void Update() {
        CheckKillRange();  
            UpdateAnimations();

        if (CanSeePlayer()) {
            _detectionMeter += Time.deltaTime / detectionTime;
            _detectionMeter = Mathf.Clamp01(_detectionMeter);
            spotLight.color = Color.Lerp(_defaultLightColour, Color.red, _detectionMeter);

            if (_detectionMeter >= 1f && _enemyState != EnemyState.Chasing) {
                _enemyState = EnemyState.Chasing;
            }
        }
        else {
            _detectionMeter -= Time.deltaTime / detectionDecayRate;
            _detectionMeter = Mathf.Clamp01(_detectionMeter);
            spotLight.color = Color.Lerp(_defaultLightColour, Color.red, _detectionMeter);

            if (_enemyState == EnemyState.Chasing) {
                _lastKnownPlayerPos = player.position;
                _searchTimer = searchTime;
                _enemyState = EnemyState.Searching;

                if (_currentMarker != null) Destroy(_currentMarker);
                _currentMarker = Instantiate(lastKnownMarkerPrefab, _lastKnownPlayerPos, quaternion.identity);
            }
        }

        UpdateDetectionUI();

        switch (_enemyState) {
            case EnemyState.Chasing:
                ChasePlayer();
                return;
            case EnemyState.Searching:
                SearchMove();
                return;
            case EnemyState.Investigating:
                InvestigateMove();
                return;
        }

        if (moveType != EnemyMoveType.Patroling) return; // Checks enemy move type is patrolling type

        // If there are no waypoints/path holders created or waypoint count is less than 2 do nothing
        if (_waypoints == null || _waypoints.Length < 2) {
            Debug.Log("Enemy patrol requires at least 2 waypoints.");
            return;
        }

        switch (_enemyState) {
            case EnemyState.Moving: PatrolMove(); break;
            default: PatrolWait(); break;
        }

        if (!_distracted)
            CheckHearingRange();
      
        }

        private void UpdateAnimations()
        {
            float speed = _agent.velocity.magnitude;

            animator.SetFloat("speed", speed);
            animator.SetBool("ismoving", speed > 0.1f);
            animator.SetBool("isChasing", _enemyState == EnemyState.Chasing);
            animator.SetBool("isInvestigating", _enemyState == EnemyState.Investigating);
            animator.SetBool("isSearching", _enemyState == EnemyState.Searching);
        }


        private void OnDrawGizmos() {
        if (pathHolder != null && pathHolder.childCount > 0) {
            var startPos = pathHolder.GetChild(0).position;
            var prevPos = startPos;

            foreach (Transform waypoint in pathHolder) {
                Gizmos.DrawSphere(waypoint.position, .3f);
                Gizmos.DrawLine(prevPos, waypoint.position);
                prevPos = waypoint.position;
            }

            Gizmos.DrawLine(prevPos, startPos);
        }

        DrawViewGizmos();
    }

    /// ------------------------------
    /// Gizmos
    /// ------------------------------
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, killRange);
    }

    private void DrawViewGizmos() {
        if (!showViewGizmos || viewDistance <= 0f) return;

        Vector3 origin = eyePosition != null ? eyePosition.position : transform.position;
        float horizontalViewAngle = spotLight != null ? spotLight.spotAngle : _viewAngle;
        if (horizontalViewAngle <= 0f) horizontalViewAngle = 60f;

        float halfHorizontal = horizontalViewAngle * 0.5f;
        float verticalLimit = Mathf.Max(0f, _maxVerticalAngle);
        int rayCount = Mathf.Max(2, viewGizmoRays);

        Gizmos.color = ViewGizmoColor;

        for (int i = 0; i <= rayCount; i++) {
            float t = i / (float)rayCount;
            float yaw = Mathf.Lerp(-halfHorizontal, halfHorizontal, t);

            Vector3 baseDirection = Quaternion.AngleAxis(yaw, Vector3.up) * transform.forward;
            Gizmos.DrawRay(origin, baseDirection.normalized * viewDistance);

            if (i == 0 || i == rayCount || i == rayCount / 2) {
                Vector3 upDirection = Quaternion.AngleAxis(-verticalLimit, transform.right) * baseDirection;
                Vector3 downDirection = Quaternion.AngleAxis(verticalLimit, transform.right) * baseDirection;
                Gizmos.DrawRay(origin, upDirection.normalized * viewDistance);
                Gizmos.DrawRay(origin, downDirection.normalized * viewDistance);
            }
        }
    }

    /// ------------------------------
    /// Detection
    /// ------------------------------
    
    private bool IsInFront(float minDot = 0.25f) {
            Vector3 toPlayer = player.position - transform.position;
            Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z).normalized;
            return Vector3.Dot(transform.forward, flat) >= minDot;
        }

    private bool CanSeePlayer() {
        if (!player || !_playerComponent) return false;
        if (_playerComponent.isHidden) return false;
        
        float proximityRadius = 1.75f;
        if (Vector3.Distance(transform.position, player.position) <= proximityRadius && IsInFront(0.1f))
        {
            Vector3 dir = (player.position - eyePosition.position).normalized;
            float dist = Vector3.Distance(eyePosition.position, player.position);
            if (!Physics.Raycast(eyePosition.position, dir, dist, viewMask, QueryTriggerInteraction.Collide))
                return true;
        }

        if (Vector3.Distance(transform.position, player.position) >= viewDistance)
            return false;

        Vector3 dirToPlayer = player.position - transform.position;
        
        Vector3 flatDirToPlayer = new Vector3(dirToPlayer.x, 0, dirToPlayer.z).normalized;
        float horizontalAngle = Vector3.Angle(transform.forward, flatDirToPlayer);
        
        if (horizontalAngle >= _viewAngle / 2f) return false;
        
        float verticalAngle = Mathf.Abs(Vector3.Angle(flatDirToPlayer, dirToPlayer.normalized));
        
        if (verticalAngle > _maxVerticalAngle) return false;

        return !Physics.Raycast(eyePosition.position, dirToPlayer, viewDistance, viewMask,
            QueryTriggerInteraction.Collide);
    }

    //detects colliders entering the hearing range
    private void CheckHearingRange() {
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position, hearingRange, _hearingHits, decoyMask);

        if (hitCount > 0 && _enemyState != EnemyState.Chasing) {
            _distracted = true;
            var decoyCollider = _hearingHits[0];
            _lastDecoyPos = decoyCollider.transform.position;

            _decoyParticles = decoyCollider.GetComponent<ParticleSystem>();
            _distractedTimer = _decoyParticles != null ? _decoyParticles.main.duration : 3f;
            _enemyState = EnemyState.Investigating;
            Debug.Log("HUUUUUUUUUUUUUUUUUUUUUHHHHHHHHH");
        }
    }

    private void UpdateDetectionUI() {
        detectionIcon.fillAmount = _detectionMeter;
        detectionIcon.gameObject.SetActive(_detectionMeter > 0f);
    }

    /// ------------------------------
    /// Combat
    /// ------------------------------
    private void ChasePlayer() {
        _lastKnownPlayerPos = player.position;
        _agent.SetDestination(player.position);
        
        TryKillPlayer();
    }

    private void CheckKillRange() {
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position, killRange, _killHits, playerMask);
        _killTarget = hitCount > 0 ? _killHits[0].gameObject : null;
    }

    private bool CanKillPlayerNow() {
        if (_killTarget == null || player == null) return false;
        if (!IsInFront(0.35f)) return false; // stricter for kill

        Vector3 toPlayer = player.position - eyePosition.position;
        float dist = toPlayer.magnitude;
        if (dist > killRange) return false;

        return !Physics.Raycast(eyePosition.position, toPlayer.normalized, dist, viewMask, QueryTriggerInteraction.Collide);
    }

    private void TryKillPlayer() {
        if (!CanKillPlayerNow()) return;
        Destroy(_killTarget);
        SceneManager.LoadScene("Scenes/You Died");
    }

    /// ------------------------------
    /// Patrol
    /// ------------------------------
    private void InitPatrol() {
        // If waypoints are null do nothing
        if (_waypoints == null || _waypoints.Length == 0) return;

        _agent.Warp(_waypoints[0]);

        _targetIndex = _waypoints.Length > 1 ? 1 : 0;

        _enemyState = EnemyState.Moving; // Set patrol state
        _waitTimer = 0f; // Guarantee no leftover state
    }

    private void CacheWaypoints() {
        // If there's no pathHolder set waypoints to null for other checks and do nothing
        if (!pathHolder) {
            Debug.Log("pathHolder not found, create a pathHolder.");
            _waypoints = null;
            return;
        }

        // Order waypoints from the order in the unity heirarchy
        var count = pathHolder.childCount;
        _waypoints = new Vector3[count]; // Allocating the waypoint array

        // Loop through each child transform of pathHolder
        for (var i = 0; i < count; i++)
            _waypoints[i] = pathHolder.GetChild(i).position;
    }

    private void PatrolMove() {
        _agent.SetDestination(_waypoints[_targetIndex]);

        if (HasArrived()) {
            _waitTimer = waitTime;
            _enemyState = EnemyState.Waiting;
        }
    }

    private bool HasArrived() {
        return !_agent.pathPending
               && _agent.remainingDistance <= _agent.stoppingDistance
               && _agent.velocity.sqrMagnitude == 0f;
    }

    private void PatrolWait() {
        _waitTimer -= Time.deltaTime;
        if (_waitTimer > 0f) return;

        // Advance to next index and wrap index back to 0
        _targetIndex = (_targetIndex + 1) % _waypoints.Length;
        _enemyState = EnemyState.Moving;
    }

    //move towards whatever has caught their attention
    private void InvestigateMove() {
        _agent.SetDestination(_lastDecoyPos);

        if (HasArrived() && !_reachedDecoy) {
            _reachedDecoy = true;
            _agent.ResetPath();
        }

        if (_reachedDecoy) {
            transform.eulerAngles += Vector3.up * (60f * Time.deltaTime);
            _distractedTimer -= Time.deltaTime;
            if (_distractedTimer <= 0f) {
                _distracted = false;
                _reachedDecoy = false;
                _enemyState = EnemyState.Moving;
            }
        }
    }

    private void SearchMove() {
        _agent.SetDestination(_lastKnownPlayerPos);

        if (HasArrived() && !_reachedSearchPoint) {
            _reachedSearchPoint = true;
            _agent.ResetPath();
        }

        if (_reachedSearchPoint) {
            transform.eulerAngles += Vector3.up * (60f * Time.deltaTime);
            _searchTimer -= Time.deltaTime;

            if (_searchTimer <= 0f) {
                _detectionMeter = 0f;
                _reachedSearchPoint = false;
                _enemyState = EnemyState.Moving;
                if (_currentMarker != null) Destroy(_currentMarker);
            }
        }
    }
}

}

