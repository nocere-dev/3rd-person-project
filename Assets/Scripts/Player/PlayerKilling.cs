using UnityEngine;
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

    private void Start() {
        indicator.SetActive(false);
    }

    private void Update() {
        canAssassinate();

        if (Input.GetMouseButtonDown(1)) Assassinating();
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

    public void canAssassinate() {
        var colliders = Physics.OverlapSphere(transform.position, killRange, enemyMask);

        if (colliders.Length > 0) {
            canKill = true;
            indicator.SetActive(true);
            target = colliders[0].gameObject;

        }
        else {
            canKill = false;
            indicator.SetActive(false);
            target = null;
        }
    }

    public void Assassinating()
    {
        if (canKill && target != null)
        {
            animator.SetTrigger("Attack");
        }
    }
    public void KillTarget()
    {
        if (target != null)
        {
            Destroy(target);
            target = null;
        }

        canKill = false;
        indicator.SetActive(false);
    }
}
