using TMPro;
using UnityEngine;

public class PlayerToolbelt : MonoBehaviour {
    public TextMeshProUGUI toolNameText;

    public Transform throwPoint;

    public Tools[] assassin_belt;

    public LayerMask playerLayer;

    [SerializeField] private int selectedToolIndex;

    private void Start() {
        toolNameText.text = assassin_belt[selectedToolIndex].name;
    }


    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            selectedToolIndex = (selectedToolIndex + 1) % assassin_belt.Length;
            toolNameText.text = assassin_belt[selectedToolIndex].name;
            Debug.Log("Selected tool: " + assassin_belt[selectedToolIndex].name);
        }

        if (Input.GetMouseButtonDown(0)) Throw(selectedToolIndex);
    }

    private void FixedUpdate() {
    }

    private void Throw(int toolIndex) {
        var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 1, 0));
        RaycastHit hit;

        var hasHit = Physics.Raycast(ray, out hit, 100f, ~playerLayer);

        var spawnPosition = throwPoint.position;
        var throwDirection = ray.direction;

        var thrownTool = Instantiate(assassin_belt[toolIndex].toolPrefab, spawnPosition, Quaternion.identity);
        var toolRb = thrownTool.GetComponent<Rigidbody>();

        if (toolRb != null) toolRb.linearVelocity = throwDirection * assassin_belt[toolIndex].throwSpeed;
    }
}