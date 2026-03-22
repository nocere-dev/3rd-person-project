using UnityEngine;
using UnityEngine.SceneManagement;

public class WaterDeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            Die();
        }
    }

    void Die()
    {
        SceneManager.LoadScene("Scenes/You Died");
    }
}