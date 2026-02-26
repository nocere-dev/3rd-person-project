using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class toolblet : MonoBehaviour
{
    public TextMeshProUGUI toolNameText;

    public Transform throwPoint;

    public Tools[] assassin_belt;

    public LayerMask playerLayer;

    [SerializeField] private int selectedToolIndex = 0;
    
    void Start()
    {
        toolNameText.text = assassin_belt[selectedToolIndex].name;
    }

    
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            selectedToolIndex = (selectedToolIndex + 1) % assassin_belt.Length;
            toolNameText.text = assassin_belt[selectedToolIndex].name;
            Debug.Log("Selected tool: " + assassin_belt[selectedToolIndex].name);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Throw(selectedToolIndex);
        }     
    }

    void FixedUpdate()
    {
        
    }

    void Throw(int toolIndex)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 1, 0));
        RaycastHit hit;

        bool hasHit = Physics.Raycast(ray, out hit, 100f, ~playerLayer);

        Vector3 spawnPosition = throwPoint.position;
        Vector3 throwDirection = ray.direction;

        GameObject thrownTool = Instantiate(assassin_belt[toolIndex].toolPrefab, spawnPosition, Quaternion.identity);
        Rigidbody toolRb = thrownTool.GetComponent<Rigidbody>();

        if (toolRb != null)
        {
            toolRb.linearVelocity = throwDirection * assassin_belt[toolIndex].throwSpeed;
        }
    }
}
