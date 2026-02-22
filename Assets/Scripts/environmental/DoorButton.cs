using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [HideInInspector]
    public bool canInteract = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            Debug.Log("inside");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            Debug.Log("outside");
        }
    }
}