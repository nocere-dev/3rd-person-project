using Unity.VisualScripting;
using UnityEngine;

public class playerPush : MonoBehaviour
{
    [SerializeField] private float pushMag = 1000f;
    private void OnControllerColliderHit(ControllerColliderHit cch)
    {
        Rigidbody rb = cch.collider.attachedRigidbody;
        if (rb != null)
        {
            Vector3 forceDirection = cch.gameObject.transform.position - transform.position;
            forceDirection.y = 0;
            forceDirection.Normalize();
            rb.AddForceAtPosition(forceDirection * pushMag, transform.position, ForceMode.Impulse);
        }
    }
}
