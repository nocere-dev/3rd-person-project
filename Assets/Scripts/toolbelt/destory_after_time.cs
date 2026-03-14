using System.Collections;
using UnityEngine;

public class destory_after_time : MonoBehaviour {
    public float timeToDestroy = 5f;

    private void Update() {
        StartCoroutine(DestroyAfterTime(timeToDestroy));
    }

    public IEnumerator DestroyAfterTime(float time) {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}