using UnityEngine;

public class ShowPressE : MonoBehaviour
{

    public GameObject pressE; 

    private void Start()
    {
        pressE.SetActive(false); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pressE.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pressE.SetActive(false);
        }
    }
}