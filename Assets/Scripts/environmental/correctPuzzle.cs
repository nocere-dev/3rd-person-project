using UnityEngine;

public class correctPuzzle : MonoBehaviour
{
    
    [Header("UI")] public GameObject pressEUI;
    [Header("UI")] public GameObject Correct;
    public Color interactColor = Color.green;
    public Renderer PlaneRenderer;
   
    [Header("Manager")]
    public Puzzle2Manager manager;
    public bool IsInteracted { get; private set; } = false; 
    
    private bool playerInRange = false;




    private void Start()
    {
        if (Correct != null)
            Correct.SetActive(false);
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
            if (Correct != null)
                    Correct.SetActive(false);
        }
    }
    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
            if (Correct != null)
                Correct.SetActive(true);
        }
    }
    
    private void Interact()
    {
        
        IsInteracted = true;

        if (PlaneRenderer != null)
            PlaneRenderer.material.color = interactColor;
        
        if (pressEUI != null)
            pressEUI.SetActive(false);
        
        if (manager != null)
            manager.CheckPuzzle();


        Debug.Log("Interaction!");
    }
}