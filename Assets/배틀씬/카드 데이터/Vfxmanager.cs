using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ������ ��� �ð� ȿ��(VFX)�� �߾ӿ��� �����ϰ� ����ϴ� �̱��� �Ŵ����Դϴ�.
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("VFX ������ ��� (������)")]
    [Tooltip("��� VFXDataSO ������ ���⿡ ����մϴ�. ����Ʈ�� ������ VFX ID�� �˴ϴ�.")]
    public List<VFXDataSO> vfxDataList;

    [Header("������ ���� ����")]
    [Tooltip("������ ���ڸ� ǥ���� UI ������. FloatingNumber.cs ��ũ��Ʈ�� �����ؾ� �մϴ�.")]
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
    /// ������ �⺻ ��ġ�� ID�� �ش��ϴ� ����Ʈ�� �������� �����Ͽ� ����մϴ�.
    /// </summary>
    public void PlayHitEffect(Vector3 basePosition, int vfxId)
    {
        if (vfxId < 0 || vfxId >= vfxDataList.Count)
        {
            Debug.LogWarning($"[VFXManager] ��ȿ���� ���� VFX ID({vfxId})�� ��û�Ǿ����ϴ�.");
            return;
        }

        VFXDataSO vfxData = vfxDataList[vfxId];
        if (vfxData == null || vfxData.particlePrefab == null)
        {
            Debug.LogWarning($"[VFXManager] vfxDataList�� {vfxId}�� �ε����� ����ְų� �������� �����ϴ�.");
            return;
        }

        // 1. SO�� ����� �������� �⺻ ��ġ�� ���Ͽ� ���� ��ġ�� ����մϴ�.
        Vector3 finalPosition = basePosition + vfxData.spawnOffset;

        // 2. ���� ��ġ�� ��ƼŬ �������� �����մϴ�.
        Instantiate(vfxData.particlePrefab, finalPosition, Quaternion.identity);
    }

    /// <summary>
    /// ������ �θ� ĵ������ Ư�� ��ġ�� ������/���� ���ڸ� ǥ���մϴ�.
    /// </summary>
    public void ShowDamageNumber(Transform parentCanvas, Vector3 position, int amount)
    {
        if (damageNumberPrefab == null)
        {
            Debug.LogError("[VFXManager] ������ ���� �������� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // UnitController�� �Ѱ��� �ڽ��� ĵ������ �θ�� �Ͽ� ������ ���ڸ� �����մϴ�.
        // parentCanvas�� null�� ���, �� �ֻ�ܿ� �����˴ϴ�.
        GameObject numberObject = Instantiate(damageNumberPrefab, parentCanvas);
        numberObject.transform.position = position;

        FloatingNumber floatingNumber = numberObject.GetComponent<FloatingNumber>();
        if (floatingNumber != null)
        {
            floatingNumber.Setup(amount);
        }
    }
}
