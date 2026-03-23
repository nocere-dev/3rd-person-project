using TMPro;
using UnityEngine;

public class PlayerToolbelt : MonoBehaviour {
    public TextMeshProUGUI toolNameText;

    public Transform throwPoint;

    public Tools[] assassin_belt;

    public LayerMask playerLayer;

    public int uses = 3;

    public TextMeshProUGUI usesIndicator;

    [SerializeField] private int selectedToolIndex;
    private CharacterController controller;
    Animator animator;
    private Vector3 _throwDirection;

    private void Start() {
        toolNameText.text = assassin_belt[selectedToolIndex].name;
        animator = GetComponent<Animator>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        usesIndicator.text = uses.ToString();
    }

    private void FixedUpdate() { }
    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Q)) {
            selectedToolIndex = (selectedToolIndex + 1) % assassin_belt.Length;
            toolNameText.text = assassin_belt[selectedToolIndex].name;
            Debug.Log("Selected tool: " + assassin_belt[selectedToolIndex].name);
        }

        if (Input.GetMouseButtonDown(0)) Throw(selectedToolIndex);
       
    }
    public void SpawnTool()
    {
        var spawnPosition = throwPoint.position;
        var throwDirection = Camera.main.transform.forward;

        var thrownTool = Instantiate(assassin_belt[selectedToolIndex].toolPrefab, spawnPosition, Quaternion.identity);
        var toolRb = thrownTool.GetComponent<Rigidbody>();

        if (toolRb != null)
            toolRb.linearVelocity = throwDirection * assassin_belt[selectedToolIndex].throwSpeed;
    }
    private void Throw(int toolIndex) {
        animator.SetTrigger("Throw");
        if (Input.GetMouseButtonDown(0) && uses > 0)
        {
            if (assassin_belt[selectedToolIndex].name != "hand")
            {
                Throw();
                uses--;
                
            }
        }
        usesIndicator.text = uses.ToString();
    }
    
    public void SpawnTool()
    {
        if (assassin_belt[selectedToolIndex].toolPrefab)
        {
            var thrownTool = Instantiate(assassin_belt[selectedToolIndex].toolPrefab, throwPoint.position, Quaternion.LookRotation(_throwDirection));
            var toolRb = thrownTool.GetComponent<Rigidbody>();
            
            if (toolRb != null)
            {
                toolRb.linearVelocity = _throwDirection * assassin_belt[selectedToolIndex].throwSpeed;
            }
        }
    }
    
    private void Throw() 
    {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 1, 0));
        RaycastHit hit;

        var hasHit = Physics.Raycast(ray, out hit, 100f, ~playerLayer);

        if (hasHit) {
            _throwDirection = (hit.point - throwPoint.position).normalized;
        }
        else {
            _throwDirection = ray.direction;
        }
        
        animator.SetTrigger("Throw");
    }
}