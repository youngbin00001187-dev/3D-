using UnityEngine;
using System;
using System.Collections.Generic;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [Header("����ü ������ ���")]
    public List<GameObject> projectilePrefabs;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void LaunchProjectile(UnitController user, GameObject targetTile, int projectileId, Action onComplete = null)
    {
        if (projectileId < 0 || projectileId >= projectilePrefabs.Count || projectilePrefabs[projectileId] == null)
        {
            Debug.LogError($"[ProjectileManager] ��ȿ���� ���� projectileId({projectileId}) �Դϴ�.");
            onComplete?.Invoke();
            return;
        }

        GameObject projectilePrefab = projectilePrefabs[projectileId];
        GameObject newProjectileObject = Instantiate(projectilePrefab, user.transform.position, Quaternion.identity);

        // ���� [�ٽ�] �θ� Ŭ���� Ÿ������ ������Ʈ�� �����ɴϴ�. ����
        AbstractProjectile projectileScript = newProjectileObject.GetComponent<AbstractProjectile>();
        if (projectileScript != null)
        {
            // �θ��� Initialize �޼��带 ȣ���ϸ�, ���������� �ڽ��� Launch�� ����˴ϴ�.
            projectileScript.Initialize(user, targetTile, onComplete);
        }
        else
        {
            Debug.LogError($"������ '{projectilePrefab.name}'�� AbstractProjectile�� ��ӹ޴� ��ũ��Ʈ�� �����ϴ�!");
            onComplete?.Invoke();
            Destroy(newProjectileObject);
        }
    }
}