using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 필요합니다.
using TMPro; // TextMeshPro를 사용하기 위해 필요합니다.

/// <summary>
/// 게임의 모든 시각 효과(VFX)를 중앙에서 관리하고 재생하는 싱글턴 매니저입니다.
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("VFX 프리팹 목록 (보관소)")]
    [Tooltip("모든 VFX 프리팹을 여기에 등록합니다. 리스트의 순서가 VFX ID가 됩니다. (0번, 1번, 2번...)")]
    public List<GameObject> vfxPrefabList;

    [Header("데미지 숫자 설정 (기존 기능)")]
    [Tooltip("데미지 숫자를 표시할 UI 프리팹")]
    public GameObject damageNumberPrefab;

    [Tooltip("데미지 숫자가 생성될 UI 캔버스")]
    public Canvas worldSpaceCanvas;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// 지정된 위치에 ID에 해당하는 이펙트를 재생합니다.
    /// </summary>
    /// <param name="position">이펙트가 생성될 월드 좌표</param>
    /// <param name="vfxId">vfxPrefabList에 등록된 이펙트의 ID (리스트 순서)</param>
    public void PlayHitEffect(Vector3 position, int vfxId)
    {
        // ▼▼▼ 여기에 디버그 로그 추가 ▼▼▼
        Debug.Log($"<color=lightblue>[VFXManager] PlayHitEffect가 ID: {vfxId} 로 호출되었습니다.</color>");
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        // ID가 유효한 범위 내에 있는지 확인합니다. (-1은 '효과 없음'을 의미)
        if (vfxId < 0 || vfxId >= vfxPrefabList.Count)
        {
            return; // 유효하지 않은 ID면 아무것도 하지 않고 함수를 종료합니다.
        }

        // ID를 이용해 리스트에서 실제 프리팹을 가져옵니다.
        GameObject effectPrefab = vfxPrefabList[vfxId];

        if (effectPrefab != null)
        {
            // 프리팹을 복제하여 이펙트를 생성합니다.
            Instantiate(effectPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"[VFXManager] vfxPrefabList의 {vfxId}번 인덱스가 비어있습니다!");
        }
    }

    /// <summary>
    /// 지정된 위치에 데미지 숫자를 표시합니다. (이 기능은 변경되지 않았습니다)
    /// </summary>
    /// <param name="position">숫자가 생성될 기준 월드 좌표</param>
    /// <param name="damage">표시할 데미지 수치</param>
    public void ShowDamageNumber(Vector3 position, int damage)
    {
        if (damageNumberPrefab != null && worldSpaceCanvas != null)
        {
            GameObject damageTextObj = Instantiate(damageNumberPrefab, worldSpaceCanvas.transform);
            damageTextObj.transform.position = position;

            TextMeshProUGUI tmp = damageTextObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = damage.ToString();
            }
        }
    }
}