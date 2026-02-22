using UnityEngine;

public GameObject promptUI;

private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        playerInRange = true;
        promptUI.SetActive(true);
    }
}

public void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        playerInRange = false;
        promptUI.SetActive(false);
    }
}