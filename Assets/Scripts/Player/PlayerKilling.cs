using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerKilling : MonoBehaviour {
    private CharacterController controller;
    Animator animator;

    public float killRange;

    public LayerMask enemyMask;

    public bool canKill;

    public GameObject indicator;

    private GameObject target;
    private Player player;

    public PlayerToolbelt toolUses;
    [SerializeField] private float killDelay = 0.35f;

    private Coroutine killRoutine;

    private void Start() {
        if (indicator != null)
        {
            indicator.SetActive(false);
        }
    }

    private void Update() {
        canAssassinate();

        bool rightClickPressed = Mouse.current != null
            ? Mouse.current.rightButton.wasPressedThisFrame
            : Input.GetMouseButtonDown(1);

        if (rightClickPressed)
        {
            Assassinating();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRange);
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        controller = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    public void KillTarget()
    {
        if (target != null)
        {
            Destroy(target);
            target = null;
            toolUses.uses++;
        }

        canKill = false;
        if (indicator != null)
        {
            indicator.SetActive(false);
        }
    }
    public void canAssassinate() {
        var colliders = Physics.OverlapSphere(transform.position, killRange, enemyMask);

        if (colliders.Length > 0) {
            canKill = true;
            if (indicator != null)
            {
                indicator.SetActive(true);
            }

            Collider closest = colliders[0];
            float closestSqrDistance = (closest.transform.position - transform.position).sqrMagnitude;
            for (int i = 1; i < colliders.Length; i++)
            {
                float sqrDistance = (colliders[i].transform.position - transform.position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closest = colliders[i];
                    closestSqrDistance = sqrDistance;
                }
            }

            target = closest.transform.root.gameObject;

        }
        else {
            canKill = false;
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
            target = null;
        }
    }

    public void Assassinating()
    {
        if (canKill && target != null)
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            if (killRoutine != null)
            {
                StopCoroutine(killRoutine);
            }

            killRoutine = StartCoroutine(DestroyAfterAnimation());

        }
    }

    IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(killDelay);
        KillTarget();
        killRoutine = null;
    }
    }
