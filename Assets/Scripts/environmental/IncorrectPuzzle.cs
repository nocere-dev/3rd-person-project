using UnityEngine;

public class IncorrectPuzzle : MonoBehaviour
{
    
    [Header("UI")] public GameObject pressEUI;
    [Header("UI")] public GameObject Wrong;
    public Color interactColor = Color.red;
    public Renderer PlaneRenderer;
    
    private bool playerInRange = false;




    private void Start()
    {
        if (Wrong != null)
            Wrong.SetActive(false);
        if (pressEUI != null)
            pressEUI.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (pressEUI != null)
                pressEUI.SetActive(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (pressEUI != null)
                pressEUI.SetActive(false);
            if (Wrong != null)
                Wrong.SetActive(false);
        }
    }
    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
            if (Wrong != null)
                Wrong.SetActive(true);
        }
    }
    
    private void Interact()
    {

        if (PlaneRenderer != null)
            PlaneRenderer.material.color = interactColor;
        
        if (pressEUI != null)
            pressEUI.SetActive(false);

        Debug.Log("Interaction!");
    }
}
