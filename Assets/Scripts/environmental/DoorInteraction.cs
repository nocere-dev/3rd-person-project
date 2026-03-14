using UnityEngine;

public class DoorInteraction : MonoBehaviour {
    [Header("Door & Box")] public GameObject door;
    public Renderer boxRenderer;
    public Color interactColor = Color.green;

    [Header("UI")] public GameObject pressEUI;

    private bool playerInRange;

    private void Start() {
        if (pressEUI != null)
            pressEUI.SetActive(false);
    }

    private void Update() {
        if (playerInRange && Input.GetKeyDown(KeyCode.E)) Interact();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            playerInRange = true;

            if (pressEUI != null)
                pressEUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            playerInRange = false;

            if (pressEUI != null)
                pressEUI.SetActive(false);
        }
    }

    private void Interact() {
        if (boxRenderer != null)
            boxRenderer.material.color = interactColor;


        if (door != null)
            door.SetActive(false);


        if (pressEUI != null)
            pressEUI.SetActive(false);

        Debug.Log("Interaction!");
    }
}