using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    public GameObject door;             
    public Renderer boxRenderer;        
    public Color interactColor = Color.green; 

   
    public DoorButton detectionScript; 

    void Update()
    {
        
        if (detectionScript != null && detectionScript.canInteract) 
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
    }

    void Interact()
    {
        
        if (boxRenderer != null)
        {
            boxRenderer.material.color = interactColor;
        }

        
        if (door != null)
        {
            door.SetActive(false);
        }
    }
}