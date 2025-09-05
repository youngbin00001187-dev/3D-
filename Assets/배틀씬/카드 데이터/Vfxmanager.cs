using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임의 모든 시각 효과(VFX)를 중앙에서 관리하고 재생하는 싱글턴 매니저입니다.
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("VFX 데이터 목록 (보관소)")]
    [Tooltip("모든 VFXDataSO 에셋을 여기에 등록합니다. 리스트의 순서가 VFX ID가 됩니다.")]
    public List<VFXDataSO> vfxDataList;

    [Header("데미지 숫자 설정")]
    [Tooltip("데미지 숫자를 표시할 UI 프리펩. FloatingNumber.cs 스크립트를 포함해야 합니다.")]
    public GameObject damageNumberPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 지정된 기본 위치에 ID에 해당하는 이펙트를 오프셋을 적용하여 재생합니다.
    /// </summary>
    public void PlayHitEffect(Vector3 basePosition, int vfxId)
    {
        if (vfxId < 0 || vfxId >= vfxDataList.Count)
        {
            Debug.LogWarning($"[VFXManager] 유효하지 않은 VFX ID({vfxId})가 요청되었습니다.");
            return;
        }

        VFXDataSO vfxData = vfxDataList[vfxId];
        if (vfxData == null || vfxData.particlePrefab == null)
        {
            Debug.LogWarning($"[VFXManager] vfxDataList의 {vfxId}번 인덱스가 비어있거나 프리펩이 없습니다.");
            return;
        }

        // 1. SO에 저장된 오프셋을 기본 위치에 더하여 최종 위치를 계산합니다.
        Vector3 finalPosition = basePosition + vfxData.spawnOffset;

        // 2. 최종 위치에 파티클 프리펩을 생성합니다.
        Instantiate(vfxData.particlePrefab, finalPosition, Quaternion.identity);
    }

    /// <summary>
    /// 지정된 부모 캔버스의 특정 위치에 데미지/힐량 숫자를 표시합니다.
    /// </summary>
    public void ShowDamageNumber(Transform parentCanvas, Vector3 position, int amount)
    {
        if (damageNumberPrefab == null)
        {
            Debug.LogError("[VFXManager] 데미지 숫자 프리펩이 할당되지 않았습니다!");
            return;
        }

        // UnitController가 넘겨준 자신의 캔버스를 부모로 하여 데미지 숫자를 생성합니다.
        // parentCanvas가 null일 경우, 씬 최상단에 생성됩니다.
        GameObject numberObject = Instantiate(damageNumberPrefab, parentCanvas);
        numberObject.transform.position = position;

        FloatingNumber floatingNumber = numberObject.GetComponent<FloatingNumber>();
        if (floatingNumber != null)
        {
            floatingNumber.Setup(amount);
        }
    }
}
