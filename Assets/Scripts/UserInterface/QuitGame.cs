using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("Quit Game"); // Just so you see something in editor
        Application.Quit();
    }
}