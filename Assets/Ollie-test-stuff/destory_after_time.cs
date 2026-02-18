using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class destory_after_time : MonoBehaviour
{
    
    void Update()
    {
        StartCoroutine(DestroyAfterTime(7f));
    }

    public IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
