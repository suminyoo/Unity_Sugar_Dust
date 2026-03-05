using UnityEngine;
using System.Collections.Generic;

public class EnemyDataManager : MonoBehaviour
{
    public static EnemyDataManager Instance;

    private Dictionary<string, EnemyData> enemyDatabase = new Dictionary<string, EnemyData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadAllEnemies();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllEnemies()
    {
        // Resources/Items 폴더 안에 있는 모든 ItemData
        EnemyData[] enemies = Resources.LoadAll<EnemyData>("Enemy");

        foreach (var enemy in enemies)
        {
            if (string.IsNullOrEmpty(enemy.enemyID))
            {
                Debug.LogWarning($"[경고] {enemy.name}의 enemyID가 비어있습니다");
                continue;
            }

            // 등록
            if (!enemyDatabase.ContainsKey(enemy.enemyID))
            {
                enemyDatabase.Add(enemy.enemyID, enemy);
            }
        }
        Debug.Log($"총 {enemyDatabase.Count}개의 아이템 데이터를 성공적으로 불러왔습니다.");
    }

    public EnemyData GetEnemyByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (enemyDatabase.TryGetValue(id, out EnemyData enemyData))
        {
            return enemyData;
        }

        Debug.LogError($"'{id}'를 가진 몬스터를 찾을 수 없습니다!");
        return null;
    }
}