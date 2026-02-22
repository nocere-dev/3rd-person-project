using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    public Vector3 boxSize = new Vector3(2, 2, 2); // size of detection box
    public LayerMask playerLayer; // make a layer for the player
    private bool playerDetected = false;

    void Update()
    {
        // Get the center of the box (this object's position)
        Vector3 center = transform.position;

        // Check for overlaps
        Collider[] hits = Physics.OverlapBox(center, boxSize / 2, Quaternion.identity, playerLayer);

        if (hits.Length > 0 && !playerDetected)
        {
            playerDetected = true;
            Debug.Log("Player entered the detection area!");
        }
        else if (hits.Length == 0 && playerDetected)
        {
            playerDetected = false; // reset when player leaves
        }
    }

    // Optional: visualize the box in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}