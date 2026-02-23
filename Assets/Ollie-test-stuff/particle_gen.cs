using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class particle_gen : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private GameObject particlePrefab;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        StartCoroutine(DespawnAfterTime(2.5f));
    }

    public void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    public IEnumerator DespawnAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    public void OnDestroy()
    {
        Instantiate(particlePrefab, transform.position, Quaternion.identity);
    }
}
