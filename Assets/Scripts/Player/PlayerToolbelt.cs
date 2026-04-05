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

    private void Start() {
        toolNameText.text = assassin_belt[selectedToolIndex].name;
        animator = GetComponent<Animator>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate() { }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            selectedToolIndex = (selectedToolIndex + 1) % assassin_belt.Length;
            toolNameText.text = assassin_belt[selectedToolIndex].name;
            Debug.Log("Selected tool: " + assassin_belt[selectedToolIndex].name);
        }

        if (Input.GetMouseButtonDown(0) && uses > 0 && assassin_belt[selectedToolIndex].name != "hand")
        {
            Throw(selectedToolIndex);
            uses--;
        }

        usesIndicator.text = uses.ToString();
    }
    private void Throw(int toolIndex)
    {
        animator.SetTrigger("Throw");
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 1, 0));
        RaycastHit hit;

        var hasHit = Physics.Raycast(ray, out hit, 100f, ~playerLayer);

        var spawnPos = throwPoint.position;
        var targetPoint = hasHit ? hit.point : (spawnPos + ray.direction * 100f);
        var throwDirection = (targetPoint - spawnPos).normalized;
        var spawnRotation = Quaternion.LookRotation(throwPoint.forward);

        var thrownTool = Instantiate(assassin_belt[toolIndex].toolPrefab, spawnPos, spawnRotation);
        var toolRb = thrownTool.GetComponent<Rigidbody>();

        if (toolRb != null)
        {
            toolRb.linearVelocity = throwDirection * assassin_belt[toolIndex].throwSpeed;
        }
    }
}