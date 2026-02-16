using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class toolblet : MonoBehaviour
{
    
    public Transform throwPoint;

    public ToolBelt[] assassin_belt;

    [SerializeField] private int selectedToolIndex = 0;

    public float throwSpeed = 10f;
    
    
    void Start()
    {
        
    }

    
    void Update()
    {
        for (int i = 0; i < assassin_belt.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedToolIndex = i;
                Debug.Log("Selected tool: " + assassin_belt[i].name);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Throw(selectedToolIndex);
        }
    }

    void Throw(int toolIndex)
    {
        
        Ray aimRay = new Ray(throwPoint.position, throwPoint.forward);
        Vector3 throwDirection = aimRay.direction;

        
        RaycastHit hit;
        bool hasHit = Physics.Raycast(aimRay, out hit, 100f);

        
        if (hasHit)
        {
            throwDirection = (hit.point - throwPoint.position).normalized;
        }

        GameObject tool = Instantiate(assassin_belt[toolIndex], throwPoint.position, Quaternion.identity);
        Rigidbody toolRb = tool.GetComponent<Rigidbody>();

        if (toolRb != null)
        {
            toolRb.linearVelocity = throwDirection * throwSpeed;
        }

        Debug.Log("Threw tool: " + assassin_belt[toolIndex].name);
    }
}
