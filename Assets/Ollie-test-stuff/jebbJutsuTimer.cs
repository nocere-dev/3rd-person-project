using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class jebbJutsuTimer : MonoBehaviour
{
    
    void Update()
    {
        StartCoroutine(DestroyAfterTime(4f));
    }
    
    public IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
