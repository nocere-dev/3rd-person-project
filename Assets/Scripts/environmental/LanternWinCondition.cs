using UnityEngine;
using UnityEngine.SceneManagement; 

public class LanternWinCondition : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject fireObject;
    public GameObject pressEUI;

    [Header("Linked Lantern for the main interactable lanterns")]
    public LanternWinCondition linkedLantern;

    private bool playerInRange;
    public bool isLit = false;

    private void Start()
    {
        if (pressEUI != null)
            pressEUI.SetActive(false);

        if (fireObject != null)
            fireObject.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isLit)
        {
            Interact();
        }
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
        }
    }

    private void Interact()
    {
        LightLantern();

       
        if (linkedLantern != null && !linkedLantern.isLit)
        {
            linkedLantern.LightLantern();
        }
    }

    public void LightLantern()
    {
        if (isLit) return; 

        isLit = true;

        if (fireObject != null)
            fireObject.SetActive(true);

        if (pressEUI != null)
            pressEUI.SetActive(false);

        CheckAllLanterns();
    }

    void CheckAllLanterns()
    {
        LanternWinCondition[] lanterns = FindObjectsOfType<LanternWinCondition>();

        foreach (LanternWinCondition l in lanterns)
        {
            if (!l.isLit)
                return; 
        }

        WinGame();
    }

    void WinGame()
    {
        SceneManager.LoadScene("WinScene");
    }
}