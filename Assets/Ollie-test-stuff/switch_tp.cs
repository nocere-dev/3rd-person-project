using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class switch_tp : MonoBehaviour
{
    public Transform playerChar;

    private Rigidbody rb;

    [SerializeField] private GameObject switchObject;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (playerChar == null)
        {
            playerChar = GameObject.FindGameObjectWithTag("Player").transform;

            Debug.Log("Foudn player char: " + playerChar.name);
        }
    }

    
    void Update()
    {
        StartCoroutine(DestroyAfterTime(2.5f));
    }

    private Vector3 lastPosition;

    public void OnCollisionEnter(Collision collision)
    {
        if (playerChar != null && collision != null && collision.contacts.Length > 0)
        {
            lastPosition = playerChar.position;
            
            Vector3 collPoint = collision.contacts[0].point;
            Collider playerCol = playerChar.GetComponent<Collider>();
            float yOffset = 1f;
            if (playerCol != null)
            {
                yOffset = playerCol.bounds.extents.y;
            }
            playerChar.position = collPoint + Vector3.up * yOffset;

            GameObject switchObj = Instantiate(switchObject, lastPosition, Quaternion.identity);

            Rigidbody playerRb = playerChar.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                try { playerRb.linearVelocity = Vector3.zero; }
                catch { playerRb.linearVelocity = Vector3.zero; }
            }
            
            Debug.Log("Player teleported to: " + collPoint);
        }

        Destroy(switchObject);
    }

    public IEnumerator DestroyAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(switchObject);
    }
}
