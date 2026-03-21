using UnityEngine;

public class PlayerKilling : MonoBehaviour {
    public float killRange;

    public LayerMask enemyMask;

    public bool canKill;

    public GameObject indicator;

    private GameObject target;

    public PlayerToolbelt toolUses;

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

    public void Assassinating() {
        if (canKill && target != null) {
            Destroy(target);

            toolUses.uses++;
            canKill = false;
            indicator.SetActive(false);
        }
    }
}