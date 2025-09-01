using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 게임의 모든 시각 효과(VFX)를 중앙에서 관리하고 재생하는 싱글턴 매니저입니다.
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("VFX 프리팹 목록 (보관소)")]
    [Tooltip("모든 VFX 프리팹을 여기에 등록합니다. 리스트의 순서가 VFX ID가 됩니다. (0번, 1번, 2번...)")]
    public List<GameObject> vfxPrefabList;

    [Header("데미지 숫자 설정")]
    [Tooltip("데미지 숫자를 표시할 UI 프리팹. FloatingNumber.cs 스크립트를 포함해야 합니다.")]
    public GameObject damageNumberPrefab;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// 지정된 위치에 ID에 해당하는 이펙트를 재생합니다.
    /// </summary>
    public void PlayHitEffect(Vector3 position, int vfxId)
    {
        if (vfxId < 0 || vfxId >= vfxPrefabList.Count)
        {
            return;
        }

        GameObject effectPrefab = vfxPrefabList[vfxId];
        if (effectPrefab != null)
        {
            Instantiate(effectPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"[VFXManager] vfxPrefabList의 {vfxId}번 인덱스가 비어있습니다!");
        }
    }

    /// <summary>
    /// 지정된 부모 캔버스의 특정 위치에 데미지/힐량 숫자를 표시합니다.
    /// </summary>
    /// <param name="parentCanvas">숫자가 생성될 부모 캔버스의 Transform. null일 경우 씬 최상단에 생성됩니다.</param>
    /// <param name="position">숫자가 생성될 기준 월드 좌표</param>
    /// <param name="amount">표시할 숫자 (음수: 데미지, 양수: 힐)</param>
    public void ShowDamageNumber(Transform parentCanvas, Vector3 position, int amount)
    {
        if (damageNumberPrefab == null)
        {
            Debug.LogError("[VFXManager] 데미지 숫자 프리펩(damageNumberPrefab)이 할당되지 않았습니다! 인스펙터에서 설정해주세요.");
            return;
        }

        GameObject numberObject = Instantiate(damageNumberPrefab, parentCanvas);
        numberObject.transform.position = position;

        FloatingNumber floatingNumber = numberObject.GetComponent<FloatingNumber>();
        if (floatingNumber != null)
        {
            floatingNumber.Setup(amount);
        }
        else
        {
            Debug.LogWarning($"'{damageNumberPrefab.name}' 프리펩에 FloatingNumber.cs 스크립트가 없습니다!", damageNumberPrefab);
        }
    }
}