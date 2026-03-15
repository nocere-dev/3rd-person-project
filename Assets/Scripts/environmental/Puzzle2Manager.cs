using UnityEngine;

public class Puzzle2Manager : MonoBehaviour {
    public correctPuzzle[] puzzlePieces;

    [Header("Door")] public GameObject door;


    public void CheckPuzzle() {
        foreach (var piece in puzzlePieces)
            if (!piece.IsInteracted)
                return;

        if (door != null) {
            door.SetActive(false);
            Debug.Log("puzzle Complete");
        }
    }
}