using System.Collections;
using UnityEngine;

public class particle_gen : MonoBehaviour {
    [SerializeField] private GameObject particlePrefab;
    private Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        StartCoroutine(DespawnAfterTime(2.5f));
    }

    public void OnDestroy() {
        Instantiate(particlePrefab, transform.position, Quaternion.identity);
    }

    public void OnCollisionEnter(Collision collision) {
        Destroy(gameObject);
    }

    public IEnumerator DespawnAfterTime(float time) {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}