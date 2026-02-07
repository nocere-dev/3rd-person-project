using UnityEngine;

public enum EnemyMoveType
{
    Static,
    Patroling
}

public enum EnemyClass
{
    Light,
    Medium,
    Heavy
}

public class Enemy : MonoBehaviour
{
    [SerializeField] private float viewDistance = 15f;
    [SerializeField] private float fov = 45f;
    public Transform eyePosition;
    
    [SerializeField] private EnemyMoveType moveType;
    [SerializeField] private EnemyClass enemyClass;
    
}
