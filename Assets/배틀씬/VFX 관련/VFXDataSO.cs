using UnityEngine;

/// <summary>
/// ���� VFX�� ������(������, ��ġ ������)�� ��� ScriptableObject�Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New VFX Data", menuName = "3DProject/VFX Data")]
public class VFXDataSO : ScriptableObject
{
    [Header("VFX ����")]
    [Tooltip("����� ��ƼŬ �ý��� �������Դϴ�.")]
    public GameObject particlePrefab;

    [Tooltip("�⺻ ���� ��ġ������ �߰����� ��ġ ������(offset)�Դϴ�.")]
    public Vector3 spawnOffset;
}
