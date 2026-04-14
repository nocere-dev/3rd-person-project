using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    public AudioSource footstepSource;
    public float movementThreshold = 0.1f;

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;

        if (speed > movementThreshold)
        {
            if (!footstepSource.isPlaying)
                footstepSource.Play();
        }
        else
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }

        lastPosition = transform.position;
    }
}