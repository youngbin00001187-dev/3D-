using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// ������ ��� �ð� ȿ��(VFX)�� �߾ӿ��� �����ϰ� ����ϴ� �̱��� �Ŵ����Դϴ�.
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("VFX ������ ��� (������)")]
    [Tooltip("��� VFX �������� ���⿡ ����մϴ�. ����Ʈ�� ������ VFX ID�� �˴ϴ�. (0��, 1��, 2��...)")]
    public List<GameObject> vfxPrefabList;

    [Header("������ ���� ����")]
    [Tooltip("������ ���ڸ� ǥ���� UI ������. FloatingNumber.cs ��ũ��Ʈ�� �����ؾ� �մϴ�.")]
    public GameObject damageNumberPrefab;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// ������ ��ġ�� ID�� �ش��ϴ� ����Ʈ�� ����մϴ�.
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
            Debug.LogWarning($"[VFXManager] vfxPrefabList�� {vfxId}�� �ε����� ����ֽ��ϴ�!");
        }
    }

    /// <summary>
    /// ������ �θ� ĵ������ Ư�� ��ġ�� ������/���� ���ڸ� ǥ���մϴ�.
    /// </summary>
    /// <param name="parentCanvas">���ڰ� ������ �θ� ĵ������ Transform. null�� ��� �� �ֻ�ܿ� �����˴ϴ�.</param>
    /// <param name="position">���ڰ� ������ ���� ���� ��ǥ</param>
    /// <param name="amount">ǥ���� ���� (����: ������, ���: ��)</param>
    public void ShowDamageNumber(Transform parentCanvas, Vector3 position, int amount)
    {
        if (damageNumberPrefab == null)
        {
            Debug.LogError("[VFXManager] ������ ���� ������(damageNumberPrefab)�� �Ҵ���� �ʾҽ��ϴ�! �ν����Ϳ��� �������ּ���.");
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
            Debug.LogWarning($"'{damageNumberPrefab.name}' �����鿡 FloatingNumber.cs ��ũ��Ʈ�� �����ϴ�!", damageNumberPrefab);
        }
    }
}