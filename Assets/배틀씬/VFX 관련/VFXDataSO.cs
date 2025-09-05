using UnityEngine;

/// <summary>
/// 개별 VFX의 데이터(프리펩, 위치 오프셋)를 담는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "New VFX Data", menuName = "3DProject/VFX Data")]
public class VFXDataSO : ScriptableObject
{
    [Header("VFX 설정")]
    [Tooltip("재생할 파티클 시스템 프리펩입니다.")]
    public GameObject particlePrefab;

    [Tooltip("기본 생성 위치에서의 추가적인 위치 보정값(offset)입니다.")]
    public Vector3 spawnOffset;
}
