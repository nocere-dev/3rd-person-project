using System;
using UnityEngine;

public class Pressure_Plate : MonoBehaviour
{
    public GameObject Door;
    
    
    
    void Start()
    {
        if(Door == null)
        {
            Debug.LogError("Door reference is missing in Pressure_Plate script on " + gameObject.name);
            return;
        }
        Door.SetActive(true);
    }

    
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("pushable"))
        {
            open();
        }
    }

    private void open()
    {
        if (Door != null)
        {
            Door.SetActive(false);
        }
    }
}
