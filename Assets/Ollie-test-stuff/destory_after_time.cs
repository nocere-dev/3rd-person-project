using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class destory_after_time : MonoBehaviour
{
    
    public float timeToDestroy = 5f;
    
    void Update()
    {
        StartCoroutine(DestroyAfterTime(timeToDestroy));
    }

    public IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
