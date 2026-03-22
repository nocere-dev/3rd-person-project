using UnityEngine;
using UnityEngine.SceneManagement;

public class TryAgain : MonoBehaviour {
    private void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void TryAgainButton() {
        SceneManager.LoadScene("Moldularenvironment");
    }
}