using UnityEngine;

[CreateAssetMenu(fileName = "Tools", menuName = "Scriptable Objects/Tools")]
public class Tools : ScriptableObject
{
    public float throwSpeed;

    public GameObject toolPrefab;

    public Rigidbody toolRb;

    void Start()
    {
        toolRb = toolPrefab.GetComponent<Rigidbody>();
    }
}
