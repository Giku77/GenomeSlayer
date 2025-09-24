using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public EnemyIds enemyId;
    public int enemyType;
    public string enemyName;
    public int health;
    public float speed;
    public int damage;
    public float attackSpeed;
    public GameObject enemyPrefab;
}
