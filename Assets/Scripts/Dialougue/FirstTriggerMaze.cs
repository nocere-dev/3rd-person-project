using UnityEngine;

public class FirstTriggerMaze : MonoBehaviour {
    [Header("UI")] public GameObject Speech1;

    private bool playerInRange;

    private void Start() {
        if (Speech1 != null)
            Speech1.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            playerInRange = true;

            if (Speech1 != null)
                Speech1.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            playerInRange = false;

            if (Speech1 != null)
                Speech1.SetActive(false);
        }
    }
}