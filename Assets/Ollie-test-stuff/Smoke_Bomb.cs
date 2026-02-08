using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Smoke_Bomb : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] public GameObject smokeEffectPrefab;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    
    void Update()
    {
        StartCoroutine(DestroyAfterTime(2.5f));
    }

    public void OnCollisionEnter(Collision collision)
    {
        Instantiate(smokeEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
