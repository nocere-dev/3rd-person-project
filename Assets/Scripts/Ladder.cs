using UnityEngine;

public class Ladder : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(other.CompareTag("Player") && other.TryGetComponent<PlayerMovement>(out var player))
            {
                player.stance = Stance.Climbing;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(other.CompareTag("Player") && other.TryGetComponent<PlayerMovement>(out var player))
            {
                player.stance = Stance.Standing;
            }
        }
    }
}
