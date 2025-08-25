using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "3DProject/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyName;
    public int maxHealth;
    public GameObject enemyPrefab; // 3D 모델 프리펩을 연결할 필드
}
