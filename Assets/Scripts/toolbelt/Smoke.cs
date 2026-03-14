using UnityEngine;

public class Smoke : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player"))
            other.GetComponent<Player>().isHidden = true;
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player"))
            other.GetComponent<Player>().isHidden = false;
    }
}