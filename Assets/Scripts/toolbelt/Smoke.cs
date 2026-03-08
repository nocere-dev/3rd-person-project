using UnityEngine;

public class Smoke : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<Player>().isHidden = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<Player>().isHidden = false;
    }
}
