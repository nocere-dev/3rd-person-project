using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class killing : MonoBehaviour
{
    public float killRange;

    public LayerMask enemyMask;

    public bool canKill;

    public GameObject indicator;
    
    private GameObject target;

    void Start()
    {
        indicator.SetActive(false);
    }

    void Update()
    {
        
        canAssassinate();
        
        if (Input.GetMouseButtonDown(1))
        {
            Assassinating();
        }
    }

    public void canAssassinate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, killRange, enemyMask);

        if(colliders.Length > 0)
        {
            canKill = true;
            indicator.SetActive(true);
            target = colliders[0].gameObject;
        }
        else
        {
            canKill = false;
            indicator.SetActive(false);
            target = null;
        }
    }

    public void Assassinating()
    {
        if(canKill && target != null)
        {
            Destroy(target);

            canKill = false;
            indicator.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRange);
    }
}
