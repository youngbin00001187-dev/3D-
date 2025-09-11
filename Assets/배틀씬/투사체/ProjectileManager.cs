using UnityEngine;
using System;
using System.Collections.Generic;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [Header("투사체 프리펩 목록")]
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
            Debug.LogError($"[ProjectileManager] 유효하지 않은 projectileId({projectileId}) 입니다.");
            onComplete?.Invoke();
            return;
        }

        GameObject projectilePrefab = projectilePrefabs[projectileId];
        GameObject newProjectileObject = Instantiate(projectilePrefab, user.transform.position, Quaternion.identity);

        // ▼▼▼ [핵심] 부모 클래스 타입으로 컴포넌트를 가져옵니다. ▼▼▼
        AbstractProjectile projectileScript = newProjectileObject.GetComponent<AbstractProjectile>();
        if (projectileScript != null)
        {
            // 부모의 Initialize 메서드를 호출하면, 내부적으로 자식의 Launch가 실행됩니다.
            projectileScript.Initialize(user, targetTile, onComplete);
        }
        else
        {
            Debug.LogError($"프리펩 '{projectilePrefab.name}'에 AbstractProjectile을 상속받는 스크립트가 없습니다!");
            onComplete?.Invoke();
            Destroy(newProjectileObject);
        }
    }
}