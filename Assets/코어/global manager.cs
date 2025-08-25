using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� ����Ǿ �ı����� �ʰ� ���� ��ü�� �����͸� �����ϴ� �߾� �������Դϴ�.
/// �� ��ũ��Ʈ�� ������ ������ '�����'�� ���Ҹ� ����մϴ�.
/// </summary>
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;

    [Header("�÷��̾� �⺻ ����")]
    [Tooltip("�÷��̾��� �ִ� ü���Դϴ�.")]
    public int playerMaxHealth = 100;
    [Tooltip("�÷��̾��� ���� ü���Դϴ�. ������ ������ �����˴ϴ�.")]
    public int playerCurrentHealth;

    [Header("�÷��̾� ���� ��Ģ")]
    [Tooltip("�� �� ���� �� �տ� ä���� ī���� ���Դϴ�.")]
    public int handSize = 5;
    [Tooltip("�� �� �־����� �ָ���(�� �ٽ� �̱�) Ƚ���Դϴ�.")]
    public int mulligansPerTurn = 1;

    // ���� ���⿡ �ִ� �׼� �� ������ �߰��߽��ϴ� ����
    [Tooltip("�÷��̾ �� �Ͽ� ������ �� �ִ� �ִ� ī��(�׼�) ���Դϴ�.")]
    public int playerMaxActionsPerTurn = 3;
    // ���������������������������

    [Header("�÷��̾� �� ����")]
    [Tooltip("�÷��̾ �����ϰ� �ִ� ��� ī�� ����Դϴ�.")]
    public List<CardDataSO> playerCardCollection = new List<CardDataSO>();
    [Tooltip("���� �������� ����� ���� ���Դϴ�.")]
    public List<CardDataSO> playerBattleDeck = new List<CardDataSO>();

    [Header("���� ���� ����")]
    [Tooltip("���� ���� ������ ����� Encounter �������Դϴ�.")]
    public EncounterSO nextEncounter;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            playerCurrentHealth = playerMaxHealth;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}